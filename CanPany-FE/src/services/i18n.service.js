// I18N Service for Frontend
class I18nService {
  constructor() {
    this.currentLanguage = localStorage.getItem('language') || 
                          (navigator.language?.startsWith('vi') ? 'vi' : 'en');
    this.translations = {};
    this.loadTranslations();
  }

  async loadTranslations() {
    try {
      // For now, use default translations directly
      // In production, you can load from API or static files if needed
      this.translations = this.getDefaultTranslations();
      
      // Optional: Try to load from API if available
      // const response = await fetch(`/api/translations/${this.currentLanguage}`);
      // if (response.ok && response.headers.get('content-type')?.includes('application/json')) {
      //   this.translations = await response.json();
      // }
    } catch (error) {
      console.error('Error loading translations:', error);
      this.translations = this.getDefaultTranslations();
    }
  }

  getDefaultTranslations() {
    const isVi = this.currentLanguage === 'vi';
    
    // Default translations as fallback
    return {
      Common: {
        Save: isVi ? 'Lưu' : 'Save',
        Cancel: isVi ? 'Hủy' : 'Cancel',
        Delete: isVi ? 'Xóa' : 'Delete',
        Edit: isVi ? 'Sửa' : 'Edit',
        View: isVi ? 'Xem' : 'View',
        Loading: isVi ? 'Đang tải...' : 'Loading...',
        Error: isVi ? 'Lỗi' : 'Error',
        Success: isVi ? 'Thành công' : 'Success',
        PleaseWait: isVi ? 'Vui lòng đợi...' : 'Please wait...',
        Confirm: isVi ? 'Xác nhận' : 'Confirm',
        Close: isVi ? 'Đóng' : 'Close',
        Back: isVi ? 'Quay lại' : 'Back',
        Next: isVi ? 'Tiếp theo' : 'Next',
        Submit: isVi ? 'Gửi' : 'Submit',
        Search: isVi ? 'Tìm kiếm' : 'Search',
        Filter: isVi ? 'Lọc' : 'Filter',
        All: isVi ? 'Tất cả' : 'All',
        NoData: isVi ? 'Không có dữ liệu' : 'No data',
        UpdateSuccess: isVi ? 'Cập nhật thành công!' : 'Update successful!',
        UpdateFailed: isVi ? 'Cập nhật thất bại!' : 'Update failed!',
        DeleteSuccess: isVi ? 'Xóa thành công!' : 'Delete successful!',
        DeleteFailed: isVi ? 'Xóa thất bại!' : 'Delete failed!',
        CreateSuccess: isVi ? 'Tạo thành công!' : 'Create successful!',
        CreateFailed: isVi ? 'Tạo thất bại!' : 'Create failed!',
        SaveSuccess: isVi ? 'Lưu thành công!' : 'Save successful!',
        SaveFailed: isVi ? 'Lưu thất bại!' : 'Save failed!',
        CannotSave: isVi ? 'Không thể lưu' : 'Cannot save',
        PleaseEnterInfo: isVi ? 'Vui lòng nhập đủ thông tin.' : 'Please enter all information.',
        PleaseEnterFullInfo: isVi ? 'Vui lòng nhập đầy đủ thông tin.' : 'Please enter full information.',
        InvalidInfo: isVi ? 'Thông tin không hợp lệ' : 'Invalid information',
        NotLoggedIn: isVi ? 'Bạn chưa đăng nhập!' : 'You are not logged in!',
        PleaseLogin: isVi ? 'Vui lòng đăng nhập' : 'Please login',
      },
      Auth: {
        Login: isVi ? 'Đăng nhập' : 'Login',
        Register: isVi ? 'Đăng ký' : 'Register',
        Logout: isVi ? 'Đăng xuất' : 'Logout',
        Email: isVi ? 'Email' : 'Email',
        Password: isVi ? 'Mật khẩu' : 'Password',
        RememberMe: isVi ? 'Ghi nhớ đăng nhập' : 'Remember me',
        ForgotPassword: isVi ? 'Quên mật khẩu?' : 'Forgot password?',
        LoginSuccess: isVi ? 'Đăng nhập thành công!' : 'Login successful!',
        LoginFailed: isVi ? 'Đăng nhập thất bại' : 'Login failed',
        RegisterSuccess: isVi ? 'Đăng ký thành công!' : 'Register successful!',
        RegisterFailed: isVi ? 'Đăng ký thất bại.' : 'Register failed.',
        LoggingIn: isVi ? 'Đang đăng nhập...' : 'Logging in...',
        WelcomeBack: isVi ? 'Chào mừng trở lại LanServe' : 'Welcome back to LanServe',
        LoginDescription: isVi ? 'Đăng nhập để quản lý dự án, trao đổi và nhận việc nhanh chóng.' : 'Login to manage projects, communicate and get jobs quickly.',
        NoAccount: isVi ? 'Chưa có tài khoản?' : "Don't have an account?",
        GoogleLoginUnavailable: isVi ? 'Google Login không khả dụng. Vui lòng đăng nhập bằng email/password.' : 'Google Login unavailable. Please login with email/password.',
        NoTokenFromServer: isVi ? 'Không nhận được token từ server' : 'No token received from server',
        NoTokenFromGoogle: isVi ? 'Không nhận được token từ Google' : 'No token received from Google',
        ResetPasswordSuccess: isVi ? 'Đặt lại mật khẩu thành công!' : 'Password reset successful!',
        ResetPasswordFailed: isVi ? 'Đặt lại mật khẩu thất bại.' : 'Password reset failed.',
        SendCodeSuccess: isVi ? 'Đã gửi mã xác nhận đến email của bạn!' : 'Verification code sent to your email!',
        CannotSendCode: isVi ? 'Không thể gửi mã xác nhận.' : 'Cannot send verification code.',
        PasswordMismatch: isVi ? 'Mật khẩu mới không khớp' : 'New passwords do not match',
        ChangePasswordSuccess: isVi ? 'Đổi mật khẩu thành công!' : 'Password changed successfully!',
        ChangePasswordFailed: isVi ? 'Đổi mật khẩu thất bại' : 'Password change failed',
        ForgotPasswordTitle: isVi ? 'Quên mật khẩu' : 'Forgot Password',
        ResetPasswordTitle: isVi ? 'Đặt lại mật khẩu' : 'Reset Password',
        ForgotPasswordDescription: isVi ? 'Nhập email để nhận mã xác nhận đặt lại mật khẩu.' : 'Enter your email to receive a password reset code.',
        SendCode: isVi ? 'Gửi mã' : 'Send Code',
        VerificationCode: isVi ? 'Mã xác nhận' : 'Verification Code',
        NewPassword: isVi ? 'Mật khẩu mới' : 'New Password',
        ChangePassword: isVi ? 'Đổi mật khẩu' : 'Change Password',
      },
      Profile: {
        Title: isVi ? 'Hồ sơ' : 'Profile',
        UpdateSuccess: isVi ? 'Cập nhật thành công!' : 'Update successful!',
        UpdateFailed: isVi ? 'Cập nhật thất bại!' : 'Update failed!',
        AddSkillFailed: isVi ? 'Thêm kỹ năng thất bại!' : 'Add skill failed!',
        RemoveSkillFailed: isVi ? 'Xóa kỹ năng thất bại!' : 'Remove skill failed!',
        HiddenProfile: isVi ? 'Người dùng đã ẩn hồ sơ công khai' : 'User has hidden public profile',
        JobTitle: isVi ? 'Chức danh' : 'Job Title',
        Location: isVi ? 'Nơi ở' : 'Location',
        HourlyRate: isVi ? 'Rate theo giờ' : 'Hourly Rate',
        NotUpdated: isVi ? 'Chưa cập nhật' : 'Not updated',
        Languages: isVi ? 'Ngôn ngữ' : 'Languages',
        AddLanguage: isVi ? '+ Thêm ngôn ngữ' : '+ Add Language',
        Certificates: isVi ? 'Chứng chỉ' : 'Certificates',
        AddCertificate: isVi ? '+ Thêm chứng chỉ' : '+ Add Certificate',
        Bio: isVi ? 'Giới thiệu' : 'Bio',
        BioPlaceholder: isVi ? 'Viết vài dòng giới thiệu về bạn...' : 'Write a few lines about yourself...',
      },
      Settings: {
        Title: isVi ? 'Cài đặt' : 'Settings',
        Avatar: isVi ? 'Ảnh đại diện' : 'Avatar',
        UpdateAvatarSuccess: isVi ? 'Cập nhật avatar thành công!' : 'Avatar updated successfully!',
        UpdateAvatarFailed: isVi ? 'Cập nhật avatar thất bại!' : 'Avatar update failed!',
        Notifications: isVi ? 'Thông báo' : 'Notifications',
        EmailNotifications: isVi ? 'Nhận thông báo qua email' : 'Receive email notifications',
        NewProjectNotifications: isVi ? 'Thông báo dự án mới' : 'New project notifications',
        MessageNotifications: isVi ? 'Thông báo tin nhắn' : 'Message notifications',
        Privacy: isVi ? 'Quyền riêng tư' : 'Privacy',
        PublicProfile: isVi ? 'Hiển thị hồ sơ công khai' : 'Show public profile',
        ShowOnlineStatus: isVi ? 'Hiển thị trạng thái online' : 'Show online status',
        CannotSaveSettings: isVi ? 'Không thể lưu cài đặt' : 'Cannot save settings',
        CannotSavePrivacy: isVi ? 'Không thể lưu cài đặt quyền riêng tư' : 'Cannot save privacy settings',
      },
      Projects: {
        Title: isVi ? 'Dự án' : 'Projects',
        MyProjects: isVi ? 'Dự án của tôi' : 'My Projects',
        CreateProject: isVi ? 'Tạo dự án mới' : 'Create New Project',
        EditProject: isVi ? 'Chỉnh sửa dự án' : 'Edit Project',
        TotalProjects: isVi ? 'Tổng dự án' : 'Total Projects',
        Open: isVi ? 'Đang mở' : 'Open',
        InProgress: isVi ? 'Đang thực hiện' : 'In Progress',
        Completed: isVi ? 'Hoàn thành' : 'Completed',
        TotalBudget: isVi ? 'Tổng ngân sách' : 'Total Budget',
        NeedFreelancer: isVi ? 'Dự án cần tuyển Freelancer' : 'Projects Need Freelancers',
        NewProject: isVi ? '+ Dự án mới' : '+ New Project',
        SearchPlaceholder: isVi ? 'Tìm dự án theo tiêu đề hoặc mô tả...' : 'Search projects by title or description...',
        PleaseEnterTitle: isVi ? 'Vui lòng nhập tiêu đề' : 'Please enter title',
        PleaseEnterDescription: isVi ? 'Vui lòng nhập mô tả' : 'Please enter description',
        PleaseSelectCategory: isVi ? 'Vui lòng chọn danh mục' : 'Please select category',
        InvalidBudget: isVi ? 'Ngân sách không hợp lệ' : 'Invalid budget',
        Saving: isVi ? 'Đang lưu...' : 'Saving...',
        CannotSendProposal: isVi ? 'Không thể gửi proposal. Thử lại sau.' : 'Cannot send proposal. Please try again later.',
        InterestedInProject: isVi ? 'Tôi quan tâm đến dự án này và muốn ứng tuyển.' : 'I am interested in this project and would like to apply.',
        ViewProject: isVi ? 'Xem dự án' : 'View Project',
        Other: isVi ? 'Khác' : 'Other',
        TitleLabel: isVi ? 'Tiêu đề' : 'Title',
        TitlePlaceholder: isVi ? 'Nhập tiêu đề dự án' : 'Enter project title',
        CategoryLabel: isVi ? 'Danh mục' : 'Category',
        SelectCategory: isVi ? '— Chọn danh mục —' : '— Select Category —',
        DescriptionLabel: isVi ? 'Mô tả' : 'Description',
        DescriptionPlaceholder: isVi ? 'Mô tả chi tiết yêu cầu...' : 'Describe detailed requirements...',
        BudgetLabel: isVi ? 'Ngân sách (đ)' : 'Budget (VND)',
        StatusLabel: isVi ? 'Trạng thái' : 'Status',
        SkillsLabel: isVi ? 'Kỹ năng' : 'Skills',
        SaveChanges: isVi ? 'Lưu thay đổi' : 'Save Changes',
        CreateProjectButton: isVi ? 'Tạo dự án' : 'Create Project',
        LoadingProjects: isVi ? 'Đang tải dự án…' : 'Loading projects...',
        NoMatchingProjects: isVi ? 'Không có dự án phù hợp.' : 'No matching projects.',
      },
      Proposals: {
        Title: isVi ? 'Đề xuất' : 'Proposals',
        NoProposals: isVi ? 'Bạn chưa có đề xuất nào' : 'You have no proposals',
        ViewProjects: isVi ? 'Xem dự án' : 'View Projects',
        Cancel: isVi ? 'Hủy' : 'Cancel',
        Cancelled: isVi ? 'Đã hủy' : 'Cancelled',
        CancelConfirm: isVi ? 'Bạn có chắc chắn muốn hủy đề xuất này?' : 'Are you sure you want to cancel this proposal?',
        CancelSuccess: isVi ? 'Đã hủy đề xuất' : 'Proposal cancelled',
        CancelFailed: isVi ? 'Không thể hủy đề xuất' : 'Cannot cancel proposal',
        EditSuccess: isVi ? 'Đã cập nhật đề xuất' : 'Proposal updated',
        EditFailed: isVi ? 'Không thể cập nhật đề xuất' : 'Cannot update proposal',
        InvalidBid: isVi ? 'Vui lòng nhập giá hợp lệ' : 'Please enter valid bid amount',
      },
      Companies: {
        Title: isVi ? 'Công ty' : 'Companies',
        CreateSuccess: isVi ? 'Đã tạo công ty thành công!' : 'Company created successfully!',
        CreateFailed: isVi ? 'Không thể tạo công ty' : 'Cannot create company',
        UpdateSuccess: isVi ? 'Đã cập nhật công ty thành công!' : 'Company updated successfully!',
        UpdateFailed: isVi ? 'Không thể cập nhật công ty' : 'Cannot update company',
      },
      CV: {
        Title: isVi ? 'CV' : 'CV',
        MyCVs: isVi ? 'CV của tôi' : 'My CVs',
        Upload: isVi ? 'Tải lên CV' : 'Upload CV',
        Primary: isVi ? 'CV chính' : 'Primary CV',
        Analyze: isVi ? 'Phân tích CV' : 'Analyze CV',
        Generate: isVi ? 'Tạo CV cho công việc' : 'Generate CV for Job',
        UploadSuccess: isVi ? 'Đã tải lên CV thành công' : 'CV uploaded successfully',
        UploadError: isVi ? 'Không thể tải lên CV' : 'Cannot upload CV',
        SetPrimarySuccess: isVi ? 'Đã đặt CV chính' : 'Primary CV set',
        SetPrimaryError: isVi ? 'Không thể đặt CV chính' : 'Cannot set primary CV',
        Analyzing: isVi ? 'Đang phân tích CV...' : 'Analyzing CV...',
        AnalysisComplete: isVi ? 'Phân tích CV hoàn tất' : 'CV analysis complete',
        AnalysisError: isVi ? 'Không thể phân tích CV' : 'Cannot analyze CV',
        DeleteSuccess: isVi ? 'Đã xóa CV' : 'CV deleted',
        DeleteError: isVi ? 'Không thể xóa CV' : 'Cannot delete CV',
        ConfirmDelete: isVi ? 'Bạn có chắc chắn muốn xóa CV này?' : 'Are you sure you want to delete this CV?',
        LoadError: isVi ? 'Không thể tải danh sách CV' : 'Cannot load CV list',
        FileTooLarge: isVi ? 'File quá lớn (tối đa 10MB)' : 'File too large (max 10MB)',
      },
      Contracts: {
        Title: isVi ? 'Hợp đồng' : 'Contracts',
        CannotLoad: isVi ? 'Không thể tải thông tin hợp đồng' : 'Cannot load contract information',
        UpdateStatusSuccess: isVi ? 'Đã cập nhật trạng thái hợp đồng' : 'Contract status updated',
        UpdateStatusConfirm: isVi ? 'Bạn có chắc chắn muốn đổi trạng thái thành "{0}"?' : 'Are you sure you want to change status to "{0}"?',
      },
      Job: {
        Title: isVi ? 'Công việc' : 'Job',
        Jobs: isVi ? 'Công việc' : 'Jobs',
        Recommended: isVi ? 'Đề xuất cho bạn' : 'Recommended for you',
        Apply: isVi ? 'Ứng tuyển' : 'Apply',
        Applied: isVi ? 'Đã ứng tuyển' : 'Applied',
      },
      AuditLogs: {
        Title: isVi ? 'Audit Logs' : 'Audit Logs',
        CannotLoad: isVi ? 'Không thể tải audit logs' : 'Cannot load audit logs',
      },
      Navbar: {
        Home: isVi ? 'Trang chủ' : 'Home',
        Projects: isVi ? 'Dự án' : 'Projects',
        PostProject: isVi ? 'Đăng dự án' : 'Post Project',
        Users: isVi ? 'Người dùng' : 'Users',
        HowItWorks: isVi ? 'Cách hoạt động' : 'How It Works',
        ViewWallet: isVi ? 'Xem ví' : 'View Wallet',
        Messages: isVi ? 'Tin nhắn' : 'Messages',
        Admin: isVi ? 'Quản trị' : 'Admin',
        Account: isVi ? 'Tài khoản' : 'Account',
        Profile: isVi ? 'Hồ sơ' : 'Profile',
        Settings: isVi ? 'Cài đặt' : 'Settings',
        Logout: isVi ? 'Đăng xuất' : 'Logout',
        Login: isVi ? 'Đăng nhập' : 'Login',
        Register: isVi ? 'Đăng ký' : 'Register',
      },
      Footer: {
        Description: isVi ? 'Nền tảng kết nối freelancer & khách hàng. Xây dựng sự nghiệp tự do và tìm kiếm dự án phù hợp.' : 'Platform connecting freelancers & clients. Build your freelance career and find suitable projects.',
        QuickLinks: isVi ? 'Liên kết nhanh' : 'Quick Links',
        FindFreelancer: isVi ? 'Tìm Freelancer' : 'Find Freelancer',
        AboutUs: isVi ? 'Về chúng tôi' : 'About Us',
        Support: isVi ? 'Hỗ trợ' : 'Support',
        HelpCenter: isVi ? 'Trung tâm trợ giúp' : 'Help Center',
        Contact: isVi ? 'Liên hệ' : 'Contact',
        Terms: isVi ? 'Điều khoản' : 'Terms',
        Privacy: isVi ? 'Bảo mật' : 'Privacy',
        FAQ: isVi ? 'FAQ' : 'FAQ',
      },
      HowItWorks: {
        Title: isVi ? 'Cách hoạt động của' : 'How',
        Subtitle: isVi ? 'Quy trình đơn giản 3 bước để hoàn thành dự án hiệu quả.' : 'Simple 3-step process to complete projects efficiently.',
        StartProject: isVi ? 'Bắt đầu dự án ngay' : 'Start Project Now',
        PostProject: isVi ? 'Đăng dự án' : 'Post Project',
        PostProjectDesc1: isVi ? 'Tạo brief rõ ràng' : 'Create clear brief',
        PostProjectDesc2: isVi ? 'Gợi ý ngân sách' : 'Suggest budget',
        PostProjectDesc3: isVi ? 'Chọn kỹ năng liên quan' : 'Select related skills',
        ChooseFreelancer: isVi ? 'Chọn freelancer' : 'Choose Freelancer',
        ChooseFreelancerDesc1: isVi ? 'Xem hồ sơ & review' : 'View profile & reviews',
        ChooseFreelancerDesc2: isVi ? 'Trao đổi trước khi chốt' : 'Discuss before closing',
        ChooseFreelancerDesc3: isVi ? 'Ký điều khoản' : 'Sign terms',
        ReceiveProduct: isVi ? 'Nhận sản phẩm' : 'Receive Product',
        ReceiveProductDesc1: isVi ? 'Theo dõi tiến độ' : 'Track progress',
        ReceiveProductDesc2: isVi ? 'Review & phản hồi' : 'Review & feedback',
        ReceiveProductDesc3: isVi ? 'Thanh toán an toàn' : 'Secure payment',
      },
      Account: {
        PersonalProfile: isVi ? 'Hồ sơ cá nhân' : 'Personal Profile',
        MyJobs: isVi ? 'Công việc của tôi' : 'My Jobs',
        MyCVs: isVi ? 'CV của tôi' : 'My CVs',
        Applications: isVi ? 'Đơn ứng tuyển' : 'Applications',
        Messages: isVi ? 'Tin nhắn' : 'Messages',
        Companies: isVi ? 'Công ty' : 'Companies',
        Settings: isVi ? 'Cài đặt' : 'Settings',
        Loading: isVi ? 'Đang tải...' : 'Loading...',
        HiddenUser: isVi ? 'Người dùng ẩn' : 'Hidden User',
        Verified: isVi ? 'Đã xác thực' : 'Verified',
        NoJobTitle: isVi ? 'Chưa có chức danh' : 'No job title',
        NoLocation: isVi ? 'Chưa rõ' : 'Unknown',
        Joined: isVi ? 'Tham gia' : 'Joined',
        Reviews: isVi ? 'đánh giá' : 'reviews',
      },
      Search: {
        Placeholder: isVi ? 'Tìm kiếm dự án theo tên, mô tả, kỹ năng…' : 'Search projects by name, description, skills...',
      },
      Notifications: {
        ProposalAccepted: isVi ? 'Đề xuất đã được chấp nhận' : 'Proposal accepted',
        NewMessage: isVi ? 'Bạn có tin nhắn mới' : 'You have a new message',
        NewProposal: isVi ? 'Đề xuất mới' : 'New proposal',
        ProposalSent: isVi ? 'Đã gửi đề xuất' : 'Proposal sent',
        NewProject: isVi ? 'Dự án mới' : 'New project',
      },
      Home: {
        HeroTitle: isVi ? 'Kết nối' : 'Connect',
        Candidate: isVi ? 'Candidate' : 'Candidate',
        Company: isVi ? 'Company' : 'Company',
        HeroSubtitle: isVi ? 'Tìm kiếm freelancer năng lực hoặc dự án phù hợp. Xây dựng sự nghiệp tự do với LanServe.' : 'Find talented freelancers or suitable projects. Build your freelance career with LanServe.',
        FindFreelancer: isVi ? 'Tìm Freelancer' : 'Find Freelancer',
        PostProject: isVi ? 'Đăng Dự Án' : 'Post Project',
        ServiceCategories: isVi ? 'Danh mục dịch vụ' : 'Service Categories',
        FeaturedFreelancers: isVi ? 'Freelancer nổi bật' : 'Featured Freelancers',
        ViewProfile: isVi ? 'Xem hồ sơ' : 'View Profile',
        ReadyToStart: isVi ? 'Sẵn sàng bắt đầu dự án của bạn?' : 'Ready to start your project?',
        SignUpFree: isVi ? 'Đăng ký miễn phí' : 'Sign Up Free',
        LearnMore: isVi ? 'Tìm hiểu thêm' : 'Learn More',
        ViewAll: isVi ? 'Xem tất cả →' : 'View All →',
        ViewDetails: isVi ? 'Xem chi tiết' : 'View Details',
        SuitableProjects: isVi ? 'Dự án phù hợp với bạn' : 'Projects suitable for you',
        MatchPercentage: isVi ? '% phù hợp' : '% match',
      },
      MyProjects: {
        Title: isVi ? 'Dự án của tôi' : 'My Projects',
        NoProjects: isVi ? 'Bạn chưa có dự án nào' : 'You have no projects',
        NoProjectsDesc: isVi ? 'Tạo dự án đầu tiên để bắt đầu tuyển Freelancer.' : 'Create your first project to start hiring freelancers.',
        CreateProject: isVi ? '+ Tạo dự án' : '+ Create Project',
        Loading: isVi ? 'Đang tải...' : 'Loading...',
      },
      NewProject: {
        Title: isVi ? 'Đăng dự án mới' : 'Post New Project',
        PostSuccess: isVi ? 'Đăng dự án thành công!' : 'Project posted successfully!',
        PostFailed: isVi ? 'Có lỗi khi đăng dự án' : 'Error posting project',
        Posting: isVi ? 'Đang đăng...' : 'Posting...',
        PostProject: isVi ? 'Đăng dự án' : 'Post Project',
        PleaseSelectDeadline: isVi ? 'Vui lòng chọn thời hạn hoàn thành.' : 'Please select deadline.',
        TitlePlaceholder: isVi ? 'VD: Thiết kế logo cho công ty công nghệ' : 'E.g: Design logo for tech company',
        DescriptionPlaceholder: isVi ? 'Mô tả chi tiết yêu cầu, mục tiêu và kỳ vọng...' : 'Describe detailed requirements, goals and expectations...',
        ProjectTitleLabel: isVi ? 'Tiêu đề dự án *' : 'Project Title *',
        CategoryLabel: isVi ? 'Danh mục *' : 'Category *',
        SelectCategory: isVi ? '--Chọn danh mục--' : '--Select Category--',
        DescriptionLabel: isVi ? 'Mô tả dự án *' : 'Project Description *',
        ProjectInfo: isVi ? 'Thông tin dự án' : 'Project Information',
        Budget: isVi ? 'Ngân sách' : 'Budget',
        BudgetType: isVi ? 'Loại ngân sách *' : 'Budget Type *',
        FixedPrice: isVi ? 'Giá cố định' : 'Fixed Price',
        HourlyRate: isVi ? 'Theo giờ' : 'Hourly',
        TotalBudget: isVi ? 'Tổng ngân sách (VND) *' : 'Total Budget (VND) *',
        Time: isVi ? 'Thời gian' : 'Time',
        Deadline: isVi ? 'Thời gian hoàn thành *' : 'Deadline *',
        RequiredSkills: isVi ? 'Kỹ năng cần thiết' : 'Required Skills',
        ProjectImages: isVi ? 'Hình ảnh dự án' : 'Project Images',
        Cancel: isVi ? 'Hủy bỏ' : 'Cancel',
        InsufficientBalance: isVi ? 'Số dư ví không đủ để chi trả cho dự án này.' : 'Insufficient wallet balance for this project.',
        Required: isVi ? 'Cần' : 'Required',
        CurrentBalance: isVi ? 'Hiện có' : 'Current Balance',
        PleaseTopUp: isVi ? 'Vui lòng nạp thêm tiền trước khi đăng dự án.' : 'Please top up your wallet before posting the project.',
      },
      Projects: {
        NeedFreelancer: isVi ? 'Dự án cần tuyển Freelancer' : 'Projects Need Freelancers',
        NewProject: isVi ? '+ Dự án mới' : '+ New Project',
      },
    };
  }

  setLanguage(lang) {
    this.currentLanguage = lang;
    localStorage.setItem('language', lang);
    this.loadTranslations();
    // Dispatch event for components to update
    window.dispatchEvent(new CustomEvent('languageChanged', { detail: lang }));
  }

  getCurrentLanguage() {
    return this.currentLanguage;
  }

  t(key, params = {}) {
    const keys = key.split('.');
    let value = this.translations;
    
    for (const k of keys) {
      value = value?.[k];
      if (!value) {
        // Try default translations
        value = this.getDefaultTranslations();
        for (const k2 of keys) {
          value = value?.[k2];
          if (!value) return key; // Return key if not found
        }
        break;
      }
    }
    
    // Replace params {0}, {1}, etc.
    if (typeof value === 'string') {
      return value.replace(/\{(\d+)\}/g, (match, index) => {
        return params[index] !== undefined ? params[index] : match;
      });
    }
    
    return value || key;
  }
}

// Export singleton instance
export const i18n = new I18nService();
export default i18n;




