using Models;
using System.Text.Json;
using AutoMapper;
using Models.ViewModels;
using Data.Repository.IRepository;
using Models.DtoModels;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace UserCore.Controllers
{
    //[Authorize(Roles = "admin,manager")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserBranchController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly int LoginUserId;

        public UserBranchController(IUnitOfWork db, IUserService userService, IMapper mapper)
        {
            _db = db;
            _userService = userService;
            _mapper = mapper;
            LoginUserId = _userService.GetUserId();
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

                var dataList = _db.UserBranch.GetAll(null, "Branch", dynamicFilters: filters);
                var dtoList = _mapper.Map<List<UserBranchDto>>(dataList);
                result.data = dtoList;
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpGet("id")]
        public IActionResult Get(int id)
        {
            Response result = new Response(true, "");
            try
            {
                var dataList = _db.UserBranch.GetAll(a => a.UserId == id, "Branch");

                var dtoList = _mapper.Map<List<UserBranchDto>>(dataList);
                result.data = dtoList;
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpGet("current")]
        public IActionResult GetCurrent()
        {
            Response result = new Response(true, "");
            try
            {
                var dataList = _db.UserBranch.GetAll(a => a.UserId == LoginUserId, "Branch");

                var dtoList = _mapper.Map<List<UserBranchDto>>(dataList);
                result.data = dtoList;
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Post(UserBranchViewModel dto)
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

                if (dto.BranchIds == null || !dto.BranchIds.Any())
                {
                    result.status = false;
                    result.message = "At least one BranchId is required.";
                    return Ok(result);
                }

                var defaultBranchId = dto.DefaultBranchId;
                if (defaultBranchId <= 0 || !dto.BranchIds.Contains(defaultBranchId))
                {
                    defaultBranchId = dto.BranchIds.First();
                }

                // 🔹 Remove existing branches for this user (important)
                var existingBranches = _db.UserBranch.GetAll(x => x.UserId == dto.UserId).ToList();

                if (existingBranches.Any())
                    _db.UserBranch.RemoveRange(existingBranches);

                // 🔹 Add new branch mappings
                foreach (var branchId in dto.BranchIds.Distinct())
                {
                    var userBranch = new UserBranch
                    {
                        UserId = dto.UserId,
                        BranchId = branchId,
                        IsDefault = branchId == defaultBranchId,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedBy = LoginUserId,
                        ModifiedBy = LoginUserId
                    };

                    _db.UserBranch.Add(userBranch);
                }

                _db.Save();

                result.message = "User branches saved successfully";
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error occurred";
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
                var obj = _db.Role.GetFirstOrDefault(a => a.Id == id);
                if (obj == null)
                {
                    result.status = false;
                    result.message = "Error while deleting";
                }
                else
                {
                    result.message = "Deleted successfully";
                    _db.Role.Remove(obj);
                    _db.Save();
                }
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }
    }
}