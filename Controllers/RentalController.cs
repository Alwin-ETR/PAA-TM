using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;

[ApiController]
[Route("api/[controller]")]
public class RentalsController : ControllerBase
{
    private readonly IConfiguration _config;

    public RentalsController(IConfiguration config)
    {
        _config = config;
    }

    private NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
    }


    [Authorize]
    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            using var conn = GetConnection();

            var data = conn.Query(@"
                SELECT r.*, u.name as user_name, c.name as car_name
                FROM Rentals r
                JOIN users u ON r.user_id = u.id
                JOIN cars c ON r.car_id = c.id");

            return Ok(new
            {
                status = "success",
                data
            });
        }
        catch
        {
            return StatusCode(500, new
            {
                status = "error",
                message = "Gagal mengambil data rental"
            });
        }
    }

    [Authorize]
    [HttpPost]
    public IActionResult Create(Rental rental)
    {
        try
        {
            using var conn = GetConnection();

            // VALIDASI USER
            var user = conn.QueryFirstOrDefault(
                "SELECT * FROM users WHERE id=@id",
                new { id = rental.User_Id });

            if (user == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "User tidak ditemukan"
                });
            }

            // VALIDASI SEPEDA
            var car = conn.QueryFirstOrDefault(
                "SELECT * FROM Cars WHERE id=@id AND deleted_at IS NULL",
                new { id = rental.Car_Id });

            if (car == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Sepeda tidak ditemukan"
                });
            }

            conn.Execute(@"
                INSERT INTO Rentals(user_id, car_id, rent_date, return_date)
                VALUES(@User_Id, @Car_Id, @Rent_Date, @Return_Date)", rental);

            return StatusCode(201, new
            {
                status = "success",
                message = "Rental berhasil ditambahkan"
            });
        }
        catch
        {
            return StatusCode(500, new
            {
                status = "error",
                message = "Gagal menambahkan rental"
            });
        }
    }
}