using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IConfiguration _config;

    public UsersController(IConfiguration config)
    {
        _config = config;
    }

    private NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            using var conn = GetConnection();

            var data = conn.Query(
                "SELECT id, name, email FROM users ORDER BY id ASC"
            );

            return Ok(new
            {
                status = "success",
                data
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "error",
                message = "Gagal mengambil data user",
                detail = ex.Message
            });
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        try
        {
            using var conn = GetConnection();

            var data = conn.QueryFirstOrDefault(
                "SELECT id, name, email FROM users WHERE id=@id",
                new { id }
            );

            if (data == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "User tidak ditemukan"
                });
            }

            return Ok(new
            {
                status = "success",
                data
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "error",
                message = "Terjadi kesalahan server",
                detail = ex.Message
            });
        }
    }


    [Authorize]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        try
        {
            using var conn = GetConnection();

            // Cek user
            var user = conn.QueryFirstOrDefault(
                "SELECT * FROM users WHERE id=@id",
                new { id }
            );

            if (user == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "User tidak ditemukan"
                });
            }

            var rental = conn.QueryFirstOrDefault(
                "SELECT * FROM rentals WHERE user_id=@id",
                new { id }
            );

            if (rental != null)
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "User tidak bisa dihapus karena masih memiliki data rental"
                });
            }

            // Hapus user
            conn.Execute("DELETE FROM users WHERE id=@id", new { id });

            return Ok(new
            {
                status = "success",
                message = "User berhasil dihapus"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "error",
                message = "Gagal menghapus user",
                detail = ex.Message
            });
        }
    }
}