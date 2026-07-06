using Models;
using Models.DtoModels;
using System.Text.Json;
using Models;
using Models.ViewModels;
using Data.Repository.IRepository;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace ProductCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationBranchController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IUserService _userService;
        private readonly int LoginUserId;
        private readonly int CompanyId;

        public LocationBranchController(IUnitOfWork db, IUserService userService)
        {
            _db = db;
            _userService = userService;
            LoginUserId = _userService.GetUserId();
            CompanyId = _userService.GetCompanyId();
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

                var dataList = _db.LocationBranch.GetAll(dynamicFilters: filters);
                result.data = dataList;
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpGet("branch/{branchId}")]
        public IActionResult GetByBranch(int branchId)
        {
            Response result = new Response(true, "");
            try
            {
                var dataList = _db.LocationBranch.GetAll(a => a.BranchId == branchId);
                result.data = dataList;
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost("upsert")]
        public IActionResult Upsert(LocationBranchViewModel dto)
        {
            Response result = new Response(true);

            try
            {
                if (!ModelState.IsValid)
                {
                    result.status = false;
                    result.message = "Invalid data";
                    return Ok(result);
                }

                // 🔹 Remove existing locations associated with this branch
                var existingLocations = _db.LocationBranch.GetAll(x => x.BranchId == dto.BranchId).ToList();

                if (existingLocations.Any())
                {
                    _db.LocationBranch.RemoveRange(existingLocations);
                }

                // 🔹 Add new location branch mappings
                foreach (var locationId in dto.LocationIds)
                {
                    var locBranch = new LocationBranch
                    {
                        CompanyId = CompanyId,
                        BranchId = dto.BranchId,
                        LocationId = locationId,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedBy = LoginUserId,
                        ModifiedBy = LoginUserId,
                        Status = true,
                        IsDeleted = false
                    };

                    _db.LocationBranch.Add(locBranch);
                }

                _db.Save();
                result.message = "Location branches saved successfully";
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error occurred";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }
    }
}