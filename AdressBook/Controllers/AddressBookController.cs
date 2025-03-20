using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Interface;
using NLog;
using ModelLayer.Model;
using ModelLayer.Entity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ModelLayer.DTO;

namespace AdressBook.Controllers;

[ApiController]
[Route("api/[controller]/")]
[Authorize]
public class AddressBookController : ControllerBase
{
    private readonly IAddressBookBL _addressBookBL;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public AddressBookController(IAddressBookBL addressBookService)
    {
        _addressBookBL = addressBookService;
    }

    private (string? Email, string Role) GetAuthenticatedUser()
    {
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

        if (string.IsNullOrEmpty(emailClaim))
            throw new UnauthorizedAccessException("Token is missing or invalid.");

        return (emailClaim, roleClaim);
    }

    [HttpPost("")]
    public IActionResult SaveAddress([FromBody] AddressRequestModel addressModel)
    {
        try
        {
            if (addressModel == null)
            {
                logger.Error("Request body is missing.");
                return BadRequest(new ResponseModel<string> { Success = false, Message = "Address details must be provided.", Data = null });
            }

            var (userId, role) = GetAuthenticatedUser();
            if (role == "Admin")

                return BadRequest(new ResponseModel<string> { Success = false, Message = "Admin cannot save personal contacts.", Data = null });

            var addressEntity = new AddressEntity
            {
                FullName = addressModel.FullName,
                Email = addressModel.Email,
                PhoneNumber = addressModel.PhoneNumber,
                Address = addressModel.Address,
                UserEmail = userId
            };

            var savedEntity = _addressBookBL.SaveAddressBookBL(addressEntity);
            if (savedEntity == null)
            {
                logger.Error("Failed to save address.");
                return StatusCode(500, new ResponseModel<string> { Success = false, Message = "Unable to save address." });
            }

            return Ok(new ResponseModel<Addresses>
            {
                Success = true,
                Message = "Address successfully created.",
                Data = new Addresses
                {
                    Id = savedEntity.Id,
                    FullName = savedEntity.FullName,
                    Email = savedEntity.Email,
                    PhoneNumber = savedEntity.PhoneNumber,
                    Address = savedEntity.Address,
                    UserEmail = savedEntity.UserEmail
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseModel<string> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAddressById(int id)
    {
        try
        {
            var (userId, role) = GetAuthenticatedUser();
            var entity = await _addressBookBL.GetAddressBookByIdBL(id, role == "Admin" ? null : userId);

            if (entity == null)
                return NotFound(new ResponseModel<string> { Success = false, Message = "Address not found In DB.", Data = null });

            return Ok(new ResponseModel<Addresses>
            {
                Success = true,
                Message = "Address retrieved successfully.",
                Data = new Addresses
                {
                    Id = entity.Id,
                    FullName = entity.FullName,
                    Email = entity.Email,
                    PhoneNumber = entity.PhoneNumber,
                    Address = entity.Address,
                    UserEmail = entity.UserEmail
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseModel<string> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllAddresses()
    {
        var (userId, role) = GetAuthenticatedUser();
        var result = await _addressBookBL.GetAllAddressBooksBL(role == "Admin" ? null : userId);

        return Ok(new ResponseModel<List<Addresses>>
        {
            Success = true,
            Message = "Fetching all saved addresses.",
            Data = result
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditAddress(int id, [FromBody] AddressRequestModel addressModel)
    {
        try
        {
            var (userId, role) = GetAuthenticatedUser();
            var addressEntity = new AddressEntity
            {
                FullName = addressModel.FullName,
                Email = addressModel.Email,
                PhoneNumber = addressModel.PhoneNumber,
                Address = addressModel.Address,
                UserEmail = userId
            };

            var result = await _addressBookBL.EditAddressBookBL(role == "Admin" ? null : userId, id, addressEntity);
            if (result == null)
                return NotFound(new ResponseModel<string> { Success = false, Message = "Address not found to update.", Data = null });


            return Ok(new ResponseModel<Addresses>
            {
                Success = true,
                Message = "Address updated successfully.",
                Data = new Addresses
                {
                    FullName = result.FullName,
                    Email = result.Email,
                    PhoneNumber = result.PhoneNumber,
                    Address = result.Address,
                    UserEmail = result.UserEmail
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseModel<string> { Success = false, Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        try
        {
            var (userId, role) = GetAuthenticatedUser();
            bool result = _addressBookBL.DeleteAddressBookBL(role == "Admin" ? null : userId, id);

            if (!result)
                return NotFound(new ResponseModel<string> { Success = false, Message = "Address not found to delete.", Data = null });

            return Ok(new ResponseModel<string> { Success = true, Message = "Address deleted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseModel<string> { Success = false, Message = ex.Message });
        }
    }
}