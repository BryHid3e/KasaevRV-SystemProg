using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_FBM.Models;
using Postgrest.Responses;
using Supabase;

#nullable enable

namespace API_FBM.Services
{
    public class UserService
    {
        private readonly AppDbContext _dbContext;
        private const string TABLE_NAME = "FBM_API"; // Название таблицы в Supabase

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                Client client = _dbContext.GetClient();
                ModeledResponse<User> response = await client.From<User>().Get();
                return response?.Models ?? new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении пользователей: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                Client client = _dbContext.GetClient();
                ModeledResponse<User> response = await client.From<User>()
                    .Where(u => u.Id == id)
                    .Get();
                
                if (response != null && response.Models.Count > 0)
                    return response.Models[0];
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении пользователя: {ex.Message}");
                return null;
            }
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Создание пользователя: {user}");
                
                // Проверяем значения полей
                Console.WriteLine($"[DEBUG] Значения полей: Name={user.Name}, Login={user.Login}, Password={user.Password}, Age={user.Age}");
                
                user.CreatedAt = DateTime.UtcNow;
                Console.WriteLine($"[DEBUG] Установлена дата создания: {user.CreatedAt}");
                
                Client client = _dbContext.GetClient();
                Console.WriteLine("[DEBUG] Клиент Supabase получен");
                
                // Создаем словарь с данными для вставки
                var userData = new Dictionary<string, object>
                {
                    { "name", user.Name ?? string.Empty },
                    { "login", user.Login ?? string.Empty },
                    { "password", user.Password ?? string.Empty },
                    { "age", user.Age },
                    { "created_at", user.CreatedAt }
                };
                
                Console.WriteLine($"[DEBUG] Подготовленные данные: {string.Join(", ", userData.Select(kv => $"{kv.Key}={kv.Value}"))}");
                
                // Прямая вставка через модель
                try
                {
                    // Используем стандартный метод From<User>
                    ModeledResponse<User> response = await client.From<User>()
                        .Insert(user);
                    
                    Console.WriteLine($"[DEBUG] Результат Insert: response={response != null}, HasModels={(response?.Models != null)}, ModelsCount={(response?.Models?.Count ?? 0)}");
                    
                    if (response?.Models != null && response.Models.Count > 0)
                    {
                        Console.WriteLine($"[DEBUG] Пользователь создан успешно: {response.Models[0]}");
                        return response.Models[0];
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Ошибка при вставке: {ex.Message}");
                }
                
                Console.WriteLine("[DEBUG] Не удалось создать пользователя");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка при создании пользователя: {ex.Message}");
                Console.WriteLine($"[ERROR] Тип исключения: {ex.GetType().FullName}");
                Console.WriteLine($"[ERROR] Стек вызовов: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ERROR] Внутреннее исключение: {ex.InnerException.Message}");
                    Console.WriteLine($"[ERROR] Тип внутреннего исключения: {ex.InnerException.GetType().FullName}");
                }
                
                return null;
            }
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            try
            {
                Client client = _dbContext.GetClient();
                ModeledResponse<User> response = await client.From<User>()
                    .Where(u => u.Id == user.Id)
                    .Update(user);
                
                if (response != null && response.Models.Count > 0)
                    return response.Models[0];
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении пользователя: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                Client client = _dbContext.GetClient();
                
                // Используем void метод и проверяем успешность выполнения
                await client.From<User>()
                    .Where(u => u.Id == id)
                    .Delete();
                
                // Проверяем, что пользователь действительно удален
                var checkUser = await GetUserByIdAsync(id);
                return checkUser == null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении пользователя: {ex.Message}");
                return false;
            }
        }

        public async Task<User?> UpdateUserNameAsync(int id, string name)
        {
            User? user = await GetUserByIdAsync(id);
            if (user == null) return null;
            
            user.Name = name;
            return await UpdateUserAsync(user);
        }

        public async Task<User?> UpdateUserLoginAsync(int id, string login)
        {
            User? user = await GetUserByIdAsync(id);
            if (user == null) return null;
            
            user.Login = login;
            return await UpdateUserAsync(user);
        }

        public async Task<User?> UpdateUserPasswordAsync(int id, string password)
        {
            User? user = await GetUserByIdAsync(id);
            if (user == null) return null;
            
            user.Password = password;
            return await UpdateUserAsync(user);
        }

        public async Task<User?> UpdateUserAgeAsync(int id, int age)
        {
            User? user = await GetUserByIdAsync(id);
            if (user == null) return null;
            
            user.Age = age;
            return await UpdateUserAsync(user);
        }

        public bool TestConnection()
        {
            try
            {
                Console.WriteLine("[DEBUG] Тестирование соединения с базой данных...");
                var client = _dbContext.GetClient();
                
                if (client == null)
                {
                    Console.WriteLine("[ERROR] Клиент Supabase не инициализирован");
                    return false;
                }
                
                Console.WriteLine("[DEBUG] Проверка доступности таблицы FBM_API...");
                var result = client.From<User>().Limit(1).Get().GetAwaiter().GetResult();
                
                Console.WriteLine($"[DEBUG] Запрос выполнен, получено записей: {result?.Models?.Count ?? 0}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка при тестировании соединения: {ex.Message}");
                Console.WriteLine($"[ERROR] Тип исключения: {ex.GetType().FullName}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ERROR] Внутреннее исключение: {ex.InnerException.Message}");
                }
                return false;
            }
        }
    }
} 