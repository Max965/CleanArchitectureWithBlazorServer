using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Application.Features.Identity.Notification;

namespace Blazor.Server.UI.Pages.Identity.Users
{
    public partial class Profile
    {

        [Inject]
        private IMediator _mediator { get; set; } = default!;
        private MudForm? _form = null !;
        private MudForm? _passwordform = null !;
        public string Title { get; set; } = "Profile";
        private bool submitting;
        [CascadingParameter]
        private Task<AuthenticationState> _authState { get; set; } = default !;
        [Inject]
        private IUploadService _uploadService { get; set; } = default !;
        private UserProfile? model { get; set; } = null!;
        private UserProfileEditValidator _userValidator = new();
        private UserManager<ApplicationUser> _userManager { get; set; } = default !;
        [Inject]
        private IIdentityService _identityService { get; set; } = default !;
        public string Id => Guid.NewGuid().ToString();
        private ChangePasswordModel _changepassword { get; set; } = new();
        private ChangePasswordModelValidator _passwordValidator = new();
        private List<OrgItem> _orgData = new();

    
        private async void ActivePanelIndexChanged(int index)
        {
            if (index == 2)
            {
                await LoadOrgData();
                await JS.InvokeVoidAsync("createOrgChart", this._orgData);
            }
        }

        private async Task LoadOrgData()
        {
            var currerntuserName = (await _authState).User.GetUserName();
            var list = await _userManager.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).Include(x => x.Superior).ToListAsync();
            foreach (var item in list)
            {
                var roles = await _userManager.GetRolesAsync(item);
                var count = await _userManager.Users.Where(x => x.SuperiorId == item.Id).CountAsync();
                var orgitem = new OrgItem();
                orgitem.id = item.Id;
                orgitem.name = item.DisplayName ?? item.UserName;
                orgitem.area = item.TenantName;
                orgitem.profileUrl = item.ProfilePictureDataUrl;
                orgitem.imageUrl = item.ProfilePictureDataUrl;
                if (currerntuserName == item.UserName)
                    orgitem.isLoggedUser = true;
                orgitem.size = "";
                orgitem.tags = item.PhoneNumber ?? item.Email;
                if (roles != null && roles.Count > 0)
                    orgitem.positionName = string.Join(',', roles);
                orgitem.parentId = item.SuperiorId;
                orgitem._directSubordinates = count;
                this._orgData.Add(orgitem);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            _userManager = ScopedServices.GetRequiredService<UserManager<ApplicationUser>>();
            var userId = (await _authState).User.GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                var userDto = await _identityService.GetApplicationUserDto(userId);
                this.model = userDto.ToUserProfile();
            }
        }

        private async Task UploadPhoto(InputFileChangeEventArgs e)
        {
            var filestream = e.File.OpenReadStream(GlobalVariable.maxAllowedSize);
            var imgstream = new MemoryStream();
            await filestream.CopyToAsync(imgstream);
            imgstream.Position = 0;
            using (var outStream = new MemoryStream())
            {
                using (var image = Image.Load(imgstream))
                {
                    image.Mutate(i => i.Resize(new ResizeOptions() { Mode = SixLabors.ImageSharp.Processing.ResizeMode.Crop, Size = new SixLabors.ImageSharp.Size(128, 128) }));
                    image.Save(outStream, SixLabors.ImageSharp.Formats.Png.PngFormat.Instance);
                    var filename = e.File.Name;
                    var fi = new FileInfo(filename);
                    var ext = fi.Extension;
                    var result = await _uploadService.UploadAsync(new UploadRequest(Guid.NewGuid().ToString() + ext, UploadType.ProfilePicture, outStream.ToArray()));
                    model.ProfilePictureDataUrl = result;
                    var user = await _userManager.FindByIdAsync(model.UserId!);
                    user!.ProfilePictureDataUrl = model.ProfilePictureDataUrl;
                    await _userManager.UpdateAsync(user);
                    Snackbar.Add(L["The avatar has been updated."], MudBlazor.Severity.Info);
                   await  _mediator.Publish(new UpdateUserProfileCommand() { UserProfile = model });
                }
            }
        }

        private async Task Submit()
        {
            submitting = true;
            try
            {
                await _form!.Validate();
                if (_form.IsValid)
                {
                    var state = await _authState;
                    var user = await _userManager.FindByIdAsync(model.UserId!);
                    user!.PhoneNumber = model.PhoneNumber;
                    user.DisplayName = model.DisplayName;
                    user.ProfilePictureDataUrl = model.ProfilePictureDataUrl;
                    await _userManager.UpdateAsync(user);
                    Snackbar.Add($"{ConstantString.UPDATESUCCESS}", MudBlazor.Severity.Info);
                    await _mediator.Publish(new UpdateUserProfileCommand() { UserProfile = model });
                }
            }
            finally
            {
                submitting = false;
            }
        }

        private async Task ChangePassword()
        {
            submitting = true;
            try
            {
                await _passwordform!.Validate();
                if (_passwordform!.IsValid)
                {
                    var user = await _userManager.FindByIdAsync(model.UserId!);
                    var result = await _userManager.ChangePasswordAsync(user!, _changepassword.CurrentPassword, _changepassword.NewPassword);
                    if (result.Succeeded)
                    {
                        Snackbar.Add($"{L["Changed password successfully."]}", MudBlazor.Severity.Info);
                    }
                    else
                    {
                        Snackbar.Add($"{string.Join(",", result.Errors.Select(x => x.Description).ToArray())}", MudBlazor.Severity.Error);
                    }
                }
            }
            finally
            {
                submitting = false;
            }
        }
        
    }
}