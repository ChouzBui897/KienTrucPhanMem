using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.Web.Areas.Configuration.Models;
using ASC.Web.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Web.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    [Authorize(Roles = "Admin")]
    public class MasterDataController : BaseController // Assuming BaseController exists
    {
        private readonly IMasterDataOperations _masterData;
        private readonly IMapper _mapper;

        public MasterDataController(IMasterDataOperations masterData, IMapper mapper)
        {
            _masterData = masterData;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> MasterKeys()
        {
            var masterKeys = await _masterData.GetAllMasterKeysAsync();
            var masterKeysViewModel = _mapper.Map<List<MasterDataKey>, List<MasterDataKeyViewModel>>(masterKeys);

            HttpContext.Session.SetSession("MasterKeys", masterKeysViewModel);

            return View(new MasterKeysViewModel
            {
                MasterKeys = masterKeysViewModel == null ? null : masterKeysViewModel.ToList(),
                IsEdit = false 
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterKeys(MasterKeysViewModel masterKeys) 
        {
            masterKeys.MasterKeys = HttpContext.Session.GetSession<List<MasterDataKeyViewModel>>("MasterKeys");

            if (!ModelState.IsValid)
            {
                return View(masterKeys);
            }

            var masterKey = _mapper.Map<MasterDataKeyViewModel, MasterDataKey>(masterKeys.MasterKeyInContext);

            if (masterKeys.IsEdit) 
            {
                await _masterData.UpdateMasterKeyAsync(masterKeys.MasterKeyInContext.PartitionKey,  masterKey); 
            }
            else
            {
                masterKey.RowKey = Guid.NewGuid().ToString();
                masterKey.PartitionKey = masterKey.Name; // Or however PartitionKey is determined
                masterKey.CreatedBy = HttpContext.User.GetCurrentUserDetails().Name;
                await _masterData.InsertMasterKeyAsync(masterKey);
            }

            return RedirectToAction("MasterKeys");
        }
        [HttpGet]
        public async Task<IActionResult> MasterValues()
        {
            ViewBag.MasterKeys = await _masterData.GetAllMasterKeysAsync();

            return View(new MasterValuesViewModel
            {
                MasterValues = new List<MasterDataValueViewModel>(),
                IsEdit = false
            });
        }
        [HttpGet]
        public async Task<IActionResult> MasterValuesByKey(string key)
        {
            // Get Master values based on master key.
            return Json(new { data = await _masterData.GetAllMasterValuesByKeyAsync(key) });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(bool isEdit, MasterDataValueViewModel masterValue) 
        {
            if (!ModelState.IsValid)
            {
                return Json("Error"); 
            }
            var masterDataValue = _mapper.Map<MasterDataValueViewModel, MasterDataValue>(masterValue);

            if (isEdit)
            {
                await _masterData.UpdateMasterValueAsync(masterDataValue.PartitionKey, masterDataValue.RowKey, masterDataValue);
            }
            else
            {
                masterDataValue.RowKey = Guid.NewGuid().ToString(); 
                await _masterData.InsertMasterValueAsync(masterDataValue);
            }
            return Json(true);
        }
    }
}
