using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Cw3.DAL;
using Cw3.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using Cw3.Middleware;
using Microsoft.AspNetCore.Authentication;
using Cw3.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Cw3
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //    services.AddAuthentication("AuthenticationBasic")
            //      .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("AuthenticationBasic", null);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = "Gakko",
                        ValidAudience = "Students",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]))
                    };
                });
            services.AddTransient<IStudentDbService, SqlServerStudentDbService>();
            services.AddSingleton<IDbService, MockDbService>();
            services.AddControllers().AddXmlSerializerFormatters();
            //Dodawanie dokumentacji
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo { Title = "Students App API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IStudentDbService service)
        {
            //Dodawanie dokumentacji cd
            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "Students App API");
            });

            app.UseMiddleware<LoggingMiddleware>();

            app.Use(async (context, next) =>
            {
                if(!context.Request.Headers.ContainsKey("Index"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Musisz podac numer indeksu.");
                    return; //short circuit
                }
                string index = context.Request.Headers["Index"].ToString();
                //exists in database
                var stud = service.GetStudent(index);
                if(stud == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("W bazie nie istnieje student o podanym indeksie.");
                    return;
                }


                await next();
            });

            app.UseRouting(); //decyduje jaki kotroler i metoda maja obsluzyc zapytanie

            app.UseAuthentication();

            app.UseAuthorization();  //sprawdza uprawnienia

            app.UseEndpoints(endpoints =>   //new StudentsController(?).GetStudents() -> Response
            {
                endpoints.MapControllers();
            });
        }
    }
}
