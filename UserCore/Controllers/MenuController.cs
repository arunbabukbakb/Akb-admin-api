
using Models;
using System.Text.Json;
using Models.DtoModels;
using Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Data.Repository.IRepository;
using AutoMapper;
using Models.DtoModels;

namespace UserCore.Controllers
{
	//[Authorize(Roles = "Admin")]
	[Route("api/[controller]")]
	[ApiController]
    
    public class MenuController : ControllerBase
	{
		private readonly IUnitOfWork _db;
        private readonly IMapper _mapper;
        public MenuController(IUnitOfWork db,IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
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

				var datalist = _db.Menu.GetAll(dynamicFilters: filters);
                var dtoList = _mapper.Map<List<MenuDto>>(datalist);
                result.data = dtoList;
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
		public IActionResult Post(MenuDto menuDto)
		{
			Response result = new Response(true);
			try
			{
				if (ModelState.IsValid)
				{
					// Create the new menu entity
					var obj = new Menu
					{
						Id = menuDto.Id,
						Title = menuDto.Title,
						Icon = menuDto.Icon,
						IsParent = menuDto.IsParent,
						ParentMenuId = menuDto.ParentMenuId == 0 ? null : menuDto.ParentMenuId,
						Path = menuDto.Path,
						OrderNumber = menuDto.OrderNumber,
						Status = menuDto.Status,
					};

					if (obj.Id == 0)
					{
						_db.Menu.Add(obj);
						result.message = "Menu Created successfully";
					}
					else
					{
						_db.Menu.Update(obj);
						result.message = "Menu Updated successfully";
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
				result.message = "Error";
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
				var obj = _db.Menu.GetFirstOrDefault(a => a.Id == id);
				if (obj == null)
				{
					result.status = false;
					result.message = "Error while deleting";
				}
				else
				{
					result.message = "Deleted successfully";
					_db.Menu.Remove(obj);
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