using System;
using Newtonsoft.Json;
using Postgrest.Models;
using Postgrest.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

#nullable enable

namespace API_FBM.Models
{
    [Table("FBM_API")]
    public class User : BaseModel
    {
        [PrimaryKey("id")]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Column("name")]
        [JsonProperty("name")]
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        public string Name { get; set; } = string.Empty;

        [Column("login")]
        [JsonProperty("login")]
        [Required(ErrorMessage = "Логин пользователя обязателен")]
        public string Login { get; set; } = string.Empty;

        [Column("password")]
        [JsonProperty("password")]
        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; } = string.Empty;

        [Column("age")]
        [JsonProperty("age")]
        [Required(ErrorMessage = "Возраст обязателен")]
        public int Age { get; set; }

        [Column("created_at")]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        // Для отладки
        public override string ToString()
        {
            return $"User: ID={Id}, Name={Name}, Login={Login}, Age={Age}, CreatedAt={CreatedAt}";
        }
    }

    // Класс, который используется для передачи данных между слоями без наследования от BaseModel
    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public static UserModel FromUser(User user)
        {
            if (user == null) return null;
            
            return new UserModel
            {
                Id = user.Id,
                Name = user.Name,
                Login = user.Login,
                Password = user.Password,
                Age = user.Age,
                CreatedAt = user.CreatedAt
            };
        }
        
        public User ToUser()
        {
            return new User
            {
                Id = Id,
                Name = Name,
                Login = Login,
                Password = Password,
                Age = Age,
                CreatedAt = CreatedAt
            };
        }
    }

    // DTO для создания пользователя
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Логин пользователя обязателен")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Возраст обязателен")]
        public int Age { get; set; }

        public User ToUser()
        {
            return new User
            {
                Name = Name,
                Login = Login,
                Password = Password,
                Age = Age,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    // UserModel как DTO для ответов API
    public class UserDto : UserModel
    {
        public static new UserDto FromUser(User user)
        {
            if (user == null) return null;
            
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Login = user.Login,
                Password = user.Password,
                Age = user.Age,
                CreatedAt = user.CreatedAt
            };
        }
    }
} 