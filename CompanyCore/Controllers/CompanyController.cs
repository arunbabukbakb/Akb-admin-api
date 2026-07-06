using Microsoft.AspNetCore.Mvc;
using Models.DtoModels;
using System.Text.Json;
using System;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Data.Repository.IRepository;
using Infrastructure.Services;
using Models;
using Models.ViewModels;
using System.IO;
using System.Threading.Tasks;

namespace CompanyCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class CompanyController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IFileHandleRepository _file;
        private readonly IUserService _userService;

        public CompanyController(IUnitOfWork db, IFileHandleRepository fileHandle, IUserService userService)
        {
            _db = db;
            _file = fileHandle;
            _userService = userService;
        }

        [HttpGet("company/{id?}")]
        public IActionResult Get(int id = 0)
        {
            Response result = new Response(true, "");
            try
            {
                Company? Company = _db.Company.GetFirstOrDefault(a => a.Id == id);

                if (Company != null)
                {
                    DateTime opentime;
                    DateTime closetime;

                    // Opening Time
                    if (!DateTime.TryParse(Company.OpeningTime, out opentime))
                    {
                        opentime = DateTime.Parse("09:00"); // default time
                    }

                    // Closing Time
                    if (!DateTime.TryParse(Company.ClosingTime, out closetime))
                    {
                        closetime = DateTime.Parse("18:00"); // default time
                    }

                    Company.OpeningTime = opentime.ToString("HH:mm", CultureInfo.InvariantCulture);
                    Company.ClosingTime = closetime.ToString("HH:mm", CultureInfo.InvariantCulture);
                    result.data = Company;
                }
                else
                {
                    result.status = false;
                    result.message = "The compony not found";
                }

                   
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }



        [HttpGet]
        public IActionResult Get(string? filter)
        {
            Response result = new Response(true, "");
            try
            {
                Dictionary<string, FilterCondition>? filters = null;
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filters = JsonSerializer.Deserialize<Dictionary<string, FilterCondition>>(filter, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                Company? Company = _db.Company.GetFirstOrDefault(a => a.Id > 0);

                if (Company != null)
                {
                    DateTime opentime;
                    DateTime closetime;

                    // Opening Time
                    if (!DateTime.TryParse(Company.OpeningTime, out opentime))
                    {
                        opentime = DateTime.Parse("09:00"); // default time
                    }

                    // Closing Time
                    if (!DateTime.TryParse(Company.ClosingTime, out closetime))
                    {
                        closetime = DateTime.Parse("18:00"); // default time
                    }

                    Company.OpeningTime = opentime.ToString("HH:mm", CultureInfo.InvariantCulture);
                    Company.ClosingTime = closetime.ToString("HH:mm", CultureInfo.InvariantCulture);
                    result.data = Company;
                }
                else
                {
                    result.status = false;
                    result.message = "The compony not found";
                }
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Company obj)
        {
            Response result = new Response(true);
            try
            {
                if (ModelState.IsValid)
                {
                    int userId = _userService.GetUserId();
                    obj.ModifiedBy = userId;
                    obj.ModifiedDate = DateTime.Now;

                    if (obj.Id == 0)
                    {
                        obj.CreatedDate = DateTime.Now;
                        obj.CreatedBy = userId;
                        _db.Company.Add(obj);
                        result.message = "Company Created successfully";
                    }
                    else
                    {
                        _db.Company.Update(obj);
                        result.message = "Company Updated successfully";
                    }
                    _db.Save();
                }

                else
                {
                    result.status = false;
                    result.message = "Invalid data";
                }
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }

        [HttpPost("upload-logo")]
        public async Task<IActionResult> UploadLogo([FromForm] CompanyLogoUploadDto model)
        {
            Response result = new Response(true);
            try
            {
                if (model.CompanyId <= 0)
                {
                    result.status = false;
                    result.message = "Invalid company ID";
                    return Ok(result);
                }

                if (model.LogoFile == null || model.LogoFile.Length == 0)
                {
                    result.status = false;
                    result.message = "No logo file provided";
                    return Ok(result);
                }

                var company = _db.Company.GetFirstOrDefault(c => c.Id == model.CompanyId);
                if (company == null)
                {
                    result.status = false;
                    result.message = "Company not found";
                    return Ok(result);
                }

                if (!string.IsNullOrEmpty(company.Logo))
                {
                    _file.RemoveFile(company.Logo, "logo");
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetExtension(model.LogoFile.FileName);
                await _file.UploadFileAsync(model.LogoFile, uniqueFileName, "logo");
                
                company.Logo = uniqueFileName;
                company.ModifiedBy = _userService.GetUserId();
                company.ModifiedDate = DateTime.Now;

                _db.Company.Update(company);
                _db.Save();

                result.message = "Logo uploaded successfully";
                result.data = company.Logo;
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error uploading logo";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Response result = new Response(true);
            try
            {
                var obj = _db.Company.GetFirstOrDefault(a => a.Id == id);
                if (obj == null)
                {
                    result.status = false;
                    result.message = "Error while deleting";
                }
                else
                {
                    result.message = "Deleted successfully";
                    _db.Company.Remove(obj);
                    _db.Save();
                }
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost("DeleteBarcode/{id}")]
        public IActionResult DeleteBarcode(int id)
        {
            Response result = new Response(true);
            try
            {
                var obj = _db.Company.GetFirstOrDefault(a => a.Id == id);
                if (obj == null)
                {
                    result.status = false;
                    result.message = "Error while deleting";
                }
                else
                {
                    result.message = "Deleted successfully";
                    _db.Company.Remove(obj);
                    _db.Save();
                }
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }
    }
}