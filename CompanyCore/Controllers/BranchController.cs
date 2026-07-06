using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Security.Claims;
using Models;
using Data.Repository.IRepository;
using Models.ViewModels;
using AutoMapper;
using Models.DtoModels;

namespace CompanyCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "admin")]
    public class BranchController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IUserService _userService;
        private readonly ILogger<BranchController> _logger;
        private readonly int LoginUserId;
        // readonly IHubContext<NotificationHub> _hubContext;
        private readonly ISettingsRepository _setting;
        private readonly IMapper _mapper;

        public BranchController(IUnitOfWork db, IUserService userService, ILogger<BranchController> logger, ISettingsRepository setting, IMapper mapper)
        {
            _db = db;
            _userService = userService;
            _logger = logger;
            // Get user details from the current user's claims
            LoginUserId = _userService.GetUserId();
            //_hubContext = hubContext;
            _setting = setting;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
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

                var dataList = _db.Branch.GetAll(null,"Company", dynamicFilters: filters);
                var dtoList = _mapper.Map<List<BranchDto>>(dataList);
                result.data = dtoList;			
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpGet("branchsearch/{search?}")]
        public IActionResult branchsearch(string? search = null)
        {
            Response result = new Response(true, "");
            try
            {
                var datas = _db.Branch.GetAll(a => EF.Functions.Like(a.Code ?? "", $"%{search}%"));
                var dtoList = _mapper.Map<List<BranchDto>>(datas);
                result.data = dtoList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred - branchsearch: {ex.Message}");
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Post(BranchViewModel model)
        {
            Branch obj = _mapper.Map<Branch>(model);
            Response result = new Response(true);
            try
            {
                if (ModelState.IsValid)
                {
                    // Automatically assign company ID of logged-in user
                    obj.CompanyId = _userService.GetCompanyId();

                    bool isNew = obj.Id == 0;
                    if (isNew)
                    {
                        obj.CreatedBy = LoginUserId;
                        obj.CreatedDate = DateTime.Now;
                        _db.Branch.Add(obj);
                    }
                    else
                    {
                        obj.ModifiedDate = DateTime.Now;
                        obj.ModifiedBy = LoginUserId;
                        _db.Branch.Update(obj);
                    }
                    _db.Save();

                    result.message = isNew ? "Branch Created successfully" : "Branch Updated successfully";
                }
                else
                {
                    result.status = false;
                    result.message = "Invalid data";
                }
                //Log
                //_userService.LogInsert("Info", "Save Branch");

            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                result.status = false;
                result.message = "Error while saving branch";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpGet("branchapprove/{id?}")]
        public IActionResult branchapprove(int id = 0)
        {
            Response result = new Response(false);
            try
            {
                if (id > 0)
                {
                    //Branch and User Approve
                    var status = _db.Branch.BranchApprove(id);
                    if (status == true)
                    {
                        _db.Save();
                        result.status = true;
                        result.message = "Branch approval success";
                    }
                    else
                    {
                        result.status = false;
                        result.message = "Invalid branch details";
                    }
                }
                else
                {
                    result.status = false;
                    result.message = "Invalid branch details";
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred - branchsearch: {ex.Message}");
                result.status = false;
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

                var obj = _db.Branch.GetFirstOrDefault(a => a.Id == id);
                if (obj == null)
                {
                    result.status = false;
                    result.message = "Error while deleting";
                }
                else
                {
                    result.status = true;
                    result.message = "Deleted successfully";
                    _db.Branch.Remove(obj);
                    _db.Save();
                }

                //Log
            //    _userService.LogInsert("Info", "Delete Branch");

            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }
    }
}