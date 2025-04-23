using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Supabase;
using System.Diagnostics;
using System.Linq;

namespace API_FBM.Models
{
    public class AppDbContext
    {
        private readonly Client _supabaseClient;
        private readonly IConfiguration _configuration;
        private bool _isInitialized = false;
        
        public AppDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            var supabaseUrl = configuration["SupabaseSetting:ApiUrl"];
            var supabaseKey = configuration["SupabaseSetting:ApiKey"];
            
            if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
            {
                throw new InvalidOperationException($"Не удалось получить настройки Supabase из конфигурации. URL: {supabaseUrl}, Key: {supabaseKey?.Length > 0}");
            }
            
            Console.WriteLine($"Connecting to Supabase at: {supabaseUrl}");
            
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };
            
            try
            {
                Console.WriteLine("Создание клиента Supabase...");
                _supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, options);
                Console.WriteLine("Клиент Supabase создан, запускаем инициализацию...");
                InitializeClient();
                Console.WriteLine("Supabase client initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Supabase client: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        
        private void InitializeClient()
        {
            try
            {
                // Инициализируем клиент синхронно
                Console.WriteLine("Начинаем инициализацию Supabase клиента...");
                _supabaseClient.InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                _isInitialized = true;
                Console.WriteLine("Supabase connection initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize Supabase client: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        
        public Client GetClient()
        {
            if (!_isInitialized)
            {
                Console.WriteLine("Client not initialized, initializing now...");
                InitializeClient();
            }
            return _supabaseClient;
        }
    }
} 