using AutoMapper;
using System.Text.Json;
using Models;
using Models.ViewModels;
using Data.Repository.IRepository;
using Models.DtoMapper;
using Models.DtoModels;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Enums;
using System.IO;
using System.Threading.Tasks;

namespace UserCore.Controllers
{
    //[Authorize(Roles = "admin,manager")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly ISettingsRepository _setting;
        private readonly IFileHandleRepository _file;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public UserController(IUnitOfWork db, ISettingsRepository settings, IFileHandleRepository file, IMapper mapper, IUserService userService)
        {
            _db = db;
            _setting = settings;
            _file = file;
            _mapper = mapper;
            _userService = userService;
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

                var dataList = User.IsInRole("admin")
                    ? _db.User.GetAll(null, "Role,Company,UserBranches,UserBranches.Branch", dynamicFilters: filters)
                    : _db.User.GetAll(u => u.Role != null && u.Role.UserType != UserTypes.admin, "Role,Company,UserBranches,UserBranches.Branch", dynamicFilters: filters);
                var dtoList = _mapper.Map<List<UserDto>>(dataList);
                result.data = dtoList;
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpGet("{userId}")]
        public IActionResult Get(int userId = 0)
        {
            Response result = new Response(true, "");
            try
            {
                var user = _db.User.GetFirstOrDefault(x => x.Id == userId, "Role,Company,UserBranches,UserBranches.Branch");
                var dto = _mapper.Map<UserDto>(user);
                result.data = dto;
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }


        [HttpPost]
        public IActionResult Post([FromBody] UserViewModel model)
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

                // Validate Company FK
                model.CompanyId = _userService.GetCompanyId();

                var user = _mapper.Map<Users>(model);

                if (model.Id == 0)
                {
                    string passwordToHash = !string.IsNullOrEmpty(model.Password) ? model.Password : "123";
                    user.Password = _setting.HashPassword(passwordToHash);

                    _db.User.Add(user);
                    _db.Save(); // Save to generate User.Id

                    if (model.BranchId.HasValue && model.BranchId.Value > 0)
                    {
                        var userBranch = new UserBranch
                        {
                            UserId = user.Id,
                            BranchId = model.BranchId.Value,
                            IsDefault = true,
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow,
                            CreatedBy = _userService.GetUserId(),
                            ModifiedBy = _userService.GetUserId()
                        };
                        _db.UserBranch.Add(userBranch);
                    }

                    result.message = "User created successfully";
                }
                else
                {
                    // UPDATE (IMPORTANT: load first)
                    var existingUser = _db.User.GetById(x => x.Id == model.Id);
                    if (existingUser == null)
                        return NotFound();

                    existingUser.UserName = user.UserName;
                    existingUser.NickName = user.NickName;
                    existingUser.Email = user.Email;
                    existingUser.Address = user.Address;
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.FullName = user.FullName;
                    existingUser.RoleId = user.RoleId;
                    existingUser.Status = user.Status;

                    // Sync UserBranch mapping
                    if (model.BranchId.HasValue && model.BranchId.Value > 0)
                    {
                        var defaultBranch = _db.UserBranch.GetFirstOrDefault(ub => ub.UserId == existingUser.Id && ub.IsDefault);
                        if (defaultBranch != null)
                        {
                            if (defaultBranch.BranchId != model.BranchId.Value)
                            {
                                defaultBranch.BranchId = model.BranchId.Value;
                                defaultBranch.ModifiedDate = DateTime.UtcNow;
                                defaultBranch.ModifiedBy = _userService.GetUserId();
                                _db.UserBranch.Update(defaultBranch);
                            }
                        }
                        else
                        {
                            var existingMapping = _db.UserBranch.GetFirstOrDefault(ub => ub.UserId == existingUser.Id && ub.BranchId == model.BranchId.Value);
                            if (existingMapping != null)
                            {
                                existingMapping.IsDefault = true;
                                existingMapping.ModifiedDate = DateTime.UtcNow;
                                existingMapping.ModifiedBy = _userService.GetUserId();
                                _db.UserBranch.Update(existingMapping);
                            }
                            else
                            {
                                var userBranch = new UserBranch
                                {
                                    UserId = existingUser.Id,
                                    BranchId = model.BranchId.Value,
                                    IsDefault = true,
                                    CreatedDate = DateTime.UtcNow,
                                    ModifiedDate = DateTime.UtcNow,
                                    CreatedBy = _userService.GetUserId(),
                                    ModifiedBy = _userService.GetUserId()
                                };
                                _db.UserBranch.Add(userBranch);
                            }
                        }
                    }

                    _db.User.Update(existingUser);
                    result.message = "User updated successfully";
                }

                _db.Save();
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error";
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }

        [HttpPost("upload-photo-sign")]
        public async Task<IActionResult> UploadPhotoSign([FromForm] UserMediaUploadDto model)
        {
            Response result = new Response(true);
            try
            {
                if (model.UserId <= 0)
                {
                    result.status = false;
                    result.message = "Invalid user ID";
                    return Ok(result);
                }

                var user = _db.User.GetFirstOrDefault(u => u.Id == model.UserId);
                if (user == null)
                {
                    result.status = false;
                    result.message = "User not found";
                    return Ok(result);
                }

                if (model.Photo != null)
                {
                    if (!string.IsNullOrEmpty(user.Photo))
                    {
                        _file.RemoveFile(user.Photo, "user/photo");
                    }
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetExtension(model.Photo.FileName);
                    await _file.UploadFileAsync(model.Photo, uniqueFileName, "user/photo");
                    user.Photo = uniqueFileName;
                }

                if (model.Sign != null)
                {
                    if (!string.IsNullOrEmpty(user.Sign))
                    {
                        _file.RemoveFile(user.Sign, "user/sign");
                    }
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetExtension(model.Sign.FileName);
                    await _file.UploadFileAsync(model.Sign, uniqueFileName, "user/sign");
                    user.Sign = uniqueFileName;
                }

                if (model.Photo != null || model.Sign != null)
                {
                    _db.User.Update(user);
                    _db.Save();
                    result.message = "Files uploaded successfully";
                    result.data = new { photo = user.Photo, sign = user.Sign };
                }
                else
                {
                    result.status = false;
                    result.message = "No files uploaded";
                }
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error uploading files";
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
                var obj = _db.User.GetFirstOrDefault(a => a.Id == id);
                if (obj == null)
                {
                    result.status = false;
                    result.message = "Error while deleting";
                }
                else
                {
                    obj.Status = false;
                    _db.User.Update(obj);
                    _db.Save();
                    result.message = "User Deactivated";
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

        [HttpPost("update-password")]
        public IActionResult UpdatePassword([FromBody] UpdatePasswordDto model)
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

                var user = _db.User.GetFirstOrDefault(u => u.Id == model.UserId);
                if (user == null)
                {
                    result.status = false;
                    result.message = "User not found.";
                    return Ok(result);
                }

                user.Password = _setting.HashPassword(model.NewPassword);
                _db.User.Update(user);
                _db.Save();

                result.message = "Password updated successfully.";
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        ////Firebase Token save/update for login user(Branch only)
        [HttpPost("UserTokenUpdate")]
        public IActionResult UserTokenUpdate(UserTokenRequest obj)
        {
            Response result = new Response(true);
            try
            {
                if (ModelState.IsValid)
                {
                    if (obj.UserId > 0 && obj.Token != null)
                    {
                        var user = _db.User.GetFirstOrDefault(a => a.Id == obj.UserId);
                        if (user != null)
                        {
                            user.FcmToken = obj.Token;
                            _db.User.Update(user);
                            _db.Save();
                        }
                    }
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
                result.message = "Error";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        /// <summary>
        /// Public self-registration endpoint.
        /// Creates the user with Status = false (inactive until admin approves).
        /// CompanyId is resolved from the first available company record.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] UserRegisterDto model)
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

                // Check for duplicate username
                var existingByUsername = _db.User.GetFirstOrDefault(u => u.UserName == model.UserName);
                if (existingByUsername != null)
                {
                    result.status = false;
                    result.message = "Username already exists. Please choose a different username.";
                    return Ok(result);
                }

                // Check for duplicate email (if provided)
                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    var existingByEmail = _db.User.GetFirstOrDefault(u => u.Email == model.Email);
                    if (existingByEmail != null)
                    {
                        result.status = false;
                        result.message = "Email is already registered. Please use a different email.";
                        return Ok(result);
                    }
                }

                // Resolve the first available company
                var company = _db.Company.GetFirstOrDefault(c => !c.IsDeleted);
                if (company == null)
                {
                    result.status = false;
                    result.message = "No company found. Please contact the administrator.";
                    return Ok(result);
                }

                // Resolve the User role dynamically from the Role table
                var userRole = _db.Role.GetFirstOrDefault(r => r.UserType == UserTypes.user);
                if (userRole == null)
                {
                    result.status = false;
                    result.message = "User role not configured. Please contact the administrator.";
                    return Ok(result);
                }

                var user = new Users
                {
                    UserName    = model.UserName,
                    Password    = _setting.HashPassword(model.Password),
                    Email       = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    NickName    = model.NickName,
                    CompanyId   = company.Id,
                    RoleId      = userRole.Id,  // Resolved from Role table (UserType = user)
                    Status      = false,        // Inactive — requires admin approval
                    CreatedDate = DateTime.UtcNow
                };

                _db.User.Add(user);
                _db.Save();

                result.status  = true;
                result.message = "Registration successful. Your account is pending admin approval.";
            }
            catch (Exception ex)
            {
                result.status        = false;
                result.message       = "Error";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }
    }
}