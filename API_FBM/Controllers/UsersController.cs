using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_FBM.Models;
using API_FBM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Linq;

#nullable enable

namespace API_FBM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly UserService _userService;
        private readonly AppDbContext _dbContext;

        public UsersController(ILogger<UsersController> logger, UserService userService, AppDbContext dbContext)
        {
            _logger = logger;
            _userService = userService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Проверка соединения с базой данных
        /// </summary>
        /// <remarks>
        /// Тестовый метод для проверки соединения с Supabase
        /// </remarks>
        [HttpGet("test-connection")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult TestConnection()
        {
            try
            {
                bool isConnected = _userService.TestConnection();
                if (isConnected)
                {
                    return Ok("Соединение с базой данных успешно установлено!");
                }
                else
                {
                    return StatusCode(500, "Не удалось подключиться к базе данных");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке соединения: {Message}", ex.Message);
                return StatusCode(500, $"Ошибка при проверке соединения: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить список всех пользователей
        /// </summary>
        /// <remarks>
        /// Возвращает полный список всех пользователей из базы данных
        /// 
        /// Пример запроса:
        ///
        ///     GET /api/Users
        ///
        /// </remarks>
        /// <returns>Список всех пользователей</returns>
        /// <response code="200">Возвращает список пользователей</response>
        /// <response code="500">Если произошла внутренняя ошибка сервера</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                
                // Преобразуем в DTO для возврата
                var result = users.Select(UserDto.FromUser);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка пользователей: {Message}", ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        /// <remarks>
        /// Возвращает информацию о конкретном пользователе по его ID
        /// 
        /// Пример запроса:
        ///
        ///     GET /api/Users/5
        ///
        /// </remarks>
        /// <param name="id">ID пользователя (целое число)</param>
        /// <returns>Пользователь с указанным ID</returns>
        /// <response code="200">Возвращает данные пользователя</response>
        /// <response code="404">Если пользователь не найден</response>
        /// <response code="500">Если произошла внутренняя ошибка сервера</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"Пользователь с ID {id} не найден");
                }
                
                // Преобразуем в DTO для возврата
                var result = UserDto.FromUser(user);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя с ID {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        /// <summary>
        /// Создать нового пользователя
        /// </summary>
        /// <remarks>
        /// Создает нового пользователя в базе данных
        /// 
        /// Пример запроса:
        ///
        ///     POST /api/Users
        ///     {
        ///        "name": "Иван Иванов",
        ///        "login": "ivan",
        ///        "password": "секретный_пароль",
        ///        "age": 30
        ///     }
        ///
        /// </remarks>
        /// <param name="userDto">Данные нового пользователя</param>
        /// <returns>Созданный пользователь с присвоенным ID</returns>
        /// <response code="201">Возвращает созданного пользователя</response>
        /// <response code="400">Если данные пользователя некорректны</response>
        /// <response code="500">Если произошла внутренняя ошибка сервера</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Ошибка валидации модели: {Errors}", 
                        string.Join("; ", ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)));
                    
                    return BadRequest(ModelState);
                }
                
                _logger.LogInformation("Создаем пользователя: {Name}, {Login}, Age: {Age}", 
                    userDto.Name, userDto.Login, userDto.Age);
                
                // Конвертируем DTO в модель User
                User user = userDto.ToUser();
                
                var createdUser = await _userService.CreateUserAsync(user);
                
                if (createdUser == null)
                {
                    _logger.LogError("Не удалось создать пользователя, возвращен null");
                    return StatusCode(500, "Не удалось создать пользователя (null result)");
                }
                
                _logger.LogInformation("Пользователь успешно создан с ID: {Id}", createdUser.Id);
                
                // Создаем DTO для возврата
                var result = UserDto.FromUser(createdUser);
                
                return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании пользователя: {Message}", ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновить данные пользователя
        /// </summary>
        /// <remarks>
        /// Полностью обновляет данные существующего пользователя
        /// 
        /// Пример запроса:
        ///
        ///     PUT /api/Users/5
        ///     {
        ///        "id": 5,
        ///        "name": "Иван Петров",
        ///        "login": "ivan_petrov",
        ///        "password": "новый_пароль",
        ///        "age": 35
        ///     }
        ///
        /// Важно: ID в URL должен совпадать с ID в теле запроса
        /// </remarks>
        /// <param name="id">ID пользователя для обновления</param>
        /// <param name="user">Новые данные пользователя</param>
        /// <returns>Обновленный пользователь</returns>
        /// <response code="200">Возвращает обновленного пользователя</response>
        /// <response code="400">Если ID в URL не соответствует ID в теле запроса</response>
        /// <response code="404">Если пользователь не найден</response>
        /// <response code="500">Если произошла внутренняя ошибка сервера</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, User user)
        {
            try
            {
                if (id != user.Id)
                {
                    return BadRequest("ID в URL не соответствует ID в теле запроса");
                }

                var existingUser = await _userService.GetUserByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound($"Пользователь с ID {id} не найден");
                }

                var updatedUser = await _userService.UpdateUserAsync(user);
                if (updatedUser == null)
                {
                    return StatusCode(500, "Не удалось обновить пользователя");
                }
                
                // Преобразуем в DTO для возврата
                var result = UserDto.FromUser(updatedUser);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении пользователя с ID {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        /// <summary>
        /// Удалить пользователя
        /// </summary>
        /// <remarks>
        /// Удаляет пользователя из базы данных по указанному ID
        /// 
        /// Пример запроса:
        ///
        ///     DELETE /api/Users/5
        ///
        /// </remarks>
        /// <param name="id">ID пользователя для удаления</param>
        /// <returns>Сообщение об успешном удалении</returns>
        /// <response code="200">Если пользователь успешно удален</response>
        /// <response code="404">Если пользователь не найден</response>
        /// <response code="500">Если произошла внутренняя ошибка сервера</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                var existingUser = await _userService.GetUserByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound($"Пользователь с ID {id} не найден");
                }

                var result = await _userService.DeleteUserAsync(id);
                if (result)
                {
                    return Ok($"Пользователь с ID {id} успешно удален");
                }
                else
                {
                    return StatusCode(500, "Не удалось удалить пользователя");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя с ID {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновить имя пользователя
        /// </summary>
        /// <remarks>
        /// Обновляет только имя существующего пользователя
        /// 
        /// Пример запроса:
        ///
        ///     PATCH /api/Users/5/name
        ///     "Новое Имя"
        ///
        /// Тело запроса - строка в кавычках с новым именем
        /// </remarks>
        /// <param name="id">ID пользователя</param>
        /// <param name="name">Новое имя пользователя</param>
        /// <returns>Обновленный пользователь</returns>
        /// <response code="200">Возвращает пользователя с обновленным именем</response>
        /// <response code="404">Если пользователь не найден</response>
        /// <response code="500">Если произошла внутренняя ошибка сервера</response>
        [HttpPatch("{id}/name")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> UpdateUserName(int id, [FromBody] string name)
        {
            try
            {
                var user = await _userService.UpdateUserNameAsync(id, name);
                if (user == null)
                {
                    return NotFound($"Пользователь с ID {id} не найден");
                }
                
                // Преобразуем в DTO для возврата
                var result = UserDto.FromUser(user);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении имени пользователя с ID {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновить логин пользователя
        /// </summary>
        /// <remarks>
        /// Обновляет только логин существующего пользователя
        /// 
        /// Пример запроса:
        ///
        ///     PATCH /api/Users/5/login
        ///     "new_login"
        ///
        /// Тело запроса - строка в кавычках с новым логином
        /// </remarks>
        /// <param name="id">ID пользователя</param>
        /// <param name="login">Новый логин пользователя</param>
        /// <returns>Обновленный пользователь</returns>
        /// <response code="200">Возвращает пользователя с обновленным логином</response>
        /// <response code="404">Если пользователь не найден</response>
        /// <response code="500">Если произошла внутренняя ошибка сервера</response>
        [HttpPatch("{id}/login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> UpdateUserLogin(int id, [FromBody] string login)
        {
            try
            {
                var user = await _userService.UpdateUserLoginAsync(id, login);
                if (user == null)
                {
                    return NotFound($"Пользователь с ID {id} не найден");
                }
                
                // Преобразуем в DTO для возврата
                var result = UserDto.FromUser(user);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении логина пользователя с ID {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновить пароль пользователя
        /// </summary>
        /// <remarks>
        /// Обновляет только пароль существующего пользователя
        /// 
        /// Пример запроса:
        ///
        ///     PATCH /api/Users/5/password
        ///     "новый_пароль"
        ///
        /// Тело запроса - строка в кавычках с новым паролем
        /// </remarks>
        /// <param name="id">ID пользователя</param>
        /// <param name="password">Новый пароль пользователя</param>
        /// <returns>Обновленный пользователь</returns>
        /// <response code="200">Возвращает пользователя с обновленным паролем</response>
        /// <response code="404">Если пользователь не найден</response>
        /// <response code="500">Если произошла внутренняя ошибка сервера</response>
        [HttpPatch("{id}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> UpdateUserPassword(int id, [FromBody] string password)
        {
            try
            {
                var user = await _userService.UpdateUserPasswordAsync(id, password);
                if (user == null)
                {
                    return NotFound($"Пользователь с ID {id} не найден");
                }
                
                // Преобразуем в DTO для возврата
                var result = UserDto.FromUser(user);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении пароля пользователя с ID {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновить возраст пользователя
        /// </summary>
        /// <remarks>
        /// Обновляет только возраст существующего пользователя
        /// 
        /// Пример запроса:
        ///
        ///     PATCH /api/Users/5/age
        ///     25
        ///
        /// Тело запроса - целое число (новый возраст)
        /// </remarks>
        /// <param name="id">ID пользователя</param>
        /// <param name="age">Новый возраст пользователя</param>
        /// <returns>Обновленный пользователь</returns>
        /// <response code="200">Возвращает пользователя с обновленным возрастом</response>
        /// <response code="404">Если пользователь не найден</response>
        /// <response code="500">Если произошла внутренняя ошибка сервера</response>
        [HttpPatch("{id}/age")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> UpdateUserAge(int id, [FromBody] int age)
        {
            try
            {
                var user = await _userService.UpdateUserAgeAsync(id, age);
                if (user == null)
                {
                    return NotFound($"Пользователь с ID {id} не найден");
                }
                
                // Преобразуем в DTO для возврата
                var result = UserDto.FromUser(user);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении возраста пользователя с ID {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }
    }
}