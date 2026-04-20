
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;

namespace RentalSepedaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SepedaController : ControllerBase
    {
        private readonly IConfiguration _config;

        public SepedaController(IConfiguration config)
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
                var data = conn.Query("SELECT * FROM Cars WHERE deleted_at IS NULL");

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
                    message = "Gagal mengambil data sepeda"
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
                    "SELECT * FROM Cars WHERE id=@id AND deleted_at IS NULL",
                    new { id });

                // VALIDASI ID
                if (data == null)
                {
                    return NotFound(new
                    {
                        status = "error",
                        message = "Data sepeda tidak ditemukan"
                    });
                }

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
                    message = "Terjadi kesalahan server"
                });
            }
        }

        [HttpPost]
        public IActionResult Create(Sepeda sepeda)
        {
            // VALIDASI INPUT
            if (string.IsNullOrEmpty(sepeda.Name))
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "Nama sepeda wajib diisi"
                });
            }

            try
            {
                using var conn = GetConnection();

                conn.Execute(
                    "INSERT INTO Cars(name, price, status) VALUES(@Name, @Price, @Status)",
                    sepeda
                );

                return StatusCode(201, new
                {
                    status = "success",
                    message = "Data sepeda berhasil ditambahkan"
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = "Gagal menambahkan data sepeda"
                });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Sepeda sepeda)
        {
            try
            {
                using var conn = GetConnection();

                conn.Execute(
                    "UPDATE Cars SET name=@Name, price=@Price, status=@Status WHERE id=@Id",
                    new { sepeda.Name, sepeda.Price, sepeda.Status, Id = id }
                );

                return Ok(new
                {
                    status = "success",
                    message = "Data sepeda berhasil diupdate"
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = "Gagal mengupdate data sepeda"
                });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                using var conn = GetConnection();

                conn.Execute(
                    "UPDATE Cars SET deleted_at = NOW() WHERE id = @id",
                    new { id }
                );

                return Ok(new
                {
                    status = "success",
                    message = "Data sepeda berhasil dihapus"
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = "Gagal menghapus data sepeda"
                });
            }
        }
    }
}