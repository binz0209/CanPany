import { useEffect, useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import apiService from "../lib/api.service";
import { jwtDecode } from "jwt-decode";
import Button from "../components/ui/button";
import Spinner from "../components/Spinner";
import { Card } from "../components/ui/card";
import Badge from "../components/ui/badge";
import Input from "../components/ui/input";
import Textarea from "../components/ui/textarea";
import { toast } from "sonner";

export default function Companies() {
  const navigate = useNavigate();
  const [companies, setCompanies] = useState([]);
  const [loading, setLoading] = useState(true);
  const [currentUserId, setCurrentUserId] = useState(null);
  const [userRole, setUserRole] = useState(null);
  const [myCompany, setMyCompany] = useState(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [companyData, setCompanyData] = useState({
    name: "",
    description: "",
    website: "",
    address: "",
    phone: "",
    email: "",
  });
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem("token") || sessionStorage.getItem("token");
    if (token) {
      try {
        const decoded = jwtDecode(token);
        setCurrentUserId(decoded.sub || decoded.nameid || decoded.userId);
        setUserRole(decoded.role || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]);
      } catch (e) {
        console.error("Error decoding token:", e);
      }
    }
  }, []);

  useEffect(() => {
    loadCompanies();
    if (currentUserId && (userRole === "Company" || userRole === "Admin")) {
      loadMyCompany();
    }
  }, [currentUserId, userRole]);

  const loadCompanies = async () => {
    try {
      setLoading(true);
      // Note: Backend doesn't have GetAll endpoint, so we'll need to implement search or list
      // For now, we'll show user's company if they have one
      setCompanies([]);
    } catch (error) {
      console.error("Error loading companies:", error);
    } finally {
      setLoading(false);
    }
  };

  const loadMyCompany = async () => {
    try {
      const data = await apiService.get("/companies/my-company");
      setMyCompany(data);
    } catch (error) {
      if (error.response?.status !== 404) {
        console.error("Error loading my company:", error);
      }
    }
  };

  const handleCreateCompany = async (e) => {
    e.preventDefault();
    try {
      setSubmitting(true);
      const data = await apiService.post("/companies", companyData);
      setMyCompany(data);
      setShowCreateForm(false);
      setCompanyData({
        name: "",
        description: "",
        website: "",
        address: "",
        phone: "",
        email: "",
      });
      toast.success("Đã tạo công ty thành công!");
    } catch (error) {
      console.error("Error creating company:", error);
      toast.error(error?.message || error?.response?.data?.message || "Không thể tạo công ty");
    } finally {
      setSubmitting(false);
    }
  };

  const handleUpdateCompany = async (e) => {
    e.preventDefault();
    try {
      setSubmitting(true);
      const data = await apiService.put(`/companies/${myCompany.id}`, companyData);
      setMyCompany(data);
      toast.success("Đã cập nhật công ty thành công!");
    } catch (error) {
      console.error("Error updating company:", error);
      toast.error(error?.message || error?.response?.data?.message || "Không thể cập nhật công ty");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="container-ld py-24">
        <Spinner />
      </div>
    );
  }

  return (
    <div className="container-ld py-8">
      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">Quản lý công ty</h1>
        <p className="text-gray-600">Quản lý thông tin công ty của bạn</p>
      </div>

      {!myCompany && !showCreateForm && (userRole === "Company" || userRole === "Admin") && (
        <Card className="p-6">
          <p className="text-center mb-4">Bạn chưa có công ty</p>
          <Button onClick={() => setShowCreateForm(true)} className="w-full">
            Tạo công ty
          </Button>
        </Card>
      )}

      {(showCreateForm || myCompany) && (
        <Card className="p-6">
          <h2 className="text-2xl font-bold mb-4">
            {myCompany ? "Cập nhật công ty" : "Tạo công ty mới"}
          </h2>
          <form
            onSubmit={myCompany ? handleUpdateCompany : handleCreateCompany}
            className="space-y-4"
          >
            <div>
              <label className="block text-sm font-medium mb-2">Tên công ty *</label>
              <Input
                value={companyData.name}
                onChange={(e) => setCompanyData({ ...companyData, name: e.target.value })}
                placeholder="Nhập tên công ty"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-2">Mô tả</label>
              <Textarea
                value={companyData.description}
                onChange={(e) =>
                  setCompanyData({ ...companyData, description: e.target.value })
                }
                placeholder="Mô tả về công ty"
                rows={4}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium mb-2">Website</label>
                <Input
                  type="url"
                  value={companyData.website}
                  onChange={(e) =>
                    setCompanyData({ ...companyData, website: e.target.value })
                  }
                  placeholder="https://example.com"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-2">Email</label>
                <Input
                  type="email"
                  value={companyData.email}
                  onChange={(e) =>
                    setCompanyData({ ...companyData, email: e.target.value })
                  }
                  placeholder="contact@example.com"
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium mb-2">Địa chỉ</label>
                <Input
                  value={companyData.address}
                  onChange={(e) =>
                    setCompanyData({ ...companyData, address: e.target.value })
                  }
                  placeholder="Địa chỉ công ty"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-2">Số điện thoại</label>
                <Input
                  value={companyData.phone}
                  onChange={(e) =>
                    setCompanyData({ ...companyData, phone: e.target.value })
                  }
                  placeholder="0123456789"
                />
              </div>
            </div>
            <div className="flex gap-2">
              <Button type="submit" disabled={submitting} className="flex-1">
                {submitting
                  ? "Đang lưu..."
                  : myCompany
                  ? "Cập nhật"
                  : "Tạo công ty"}
              </Button>
              {showCreateForm && (
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => {
                    setShowCreateForm(false);
                    setCompanyData({
                      name: "",
                      description: "",
                      website: "",
                      address: "",
                      phone: "",
                      email: "",
                    });
                  }}
                >
                  Hủy
                </Button>
              )}
            </div>
          </form>

          {myCompany && (
            <div className="mt-6 border-t pt-6">
              <h3 className="font-semibold mb-2">Trạng thái xác thực</h3>
              <Badge
                variant={myCompany.isVerified ? "default" : "secondary"}
                className="mb-4"
              >
                {myCompany.isVerified ? "Đã xác thực" : "Chưa xác thực"}
              </Badge>
              {!myCompany.isVerified && (
                <div>
                  <p className="text-sm text-gray-600 mb-2">
                    Gửi yêu cầu xác thực để được đánh dấu là công ty đã được xác thực
                  </p>
                  <Button
                    variant="outline"
                    onClick={async () => {
                      try {
                        await axios.post(`/companies/${myCompany.id}/verification-request`, []);
                        toast.success("Đã gửi yêu cầu xác thực");
                      } catch (error) {
                        console.error("Error requesting verification:", error);
                        toast.error("Không thể gửi yêu cầu xác thực");
                      }
                    }}
                  >
                    Gửi yêu cầu xác thực
                  </Button>
                </div>
              )}
            </div>
          )}
        </Card>
      )}

      {myCompany && (
        <Card className="p-6 mt-6">
          <h3 className="font-semibold mb-4">Thông tin công ty</h3>
          <div className="space-y-2 text-sm">
            <div>
              <span className="text-gray-600">Tên:</span>
              <span className="ml-2 font-semibold">{myCompany.name}</span>
            </div>
            {myCompany.description && (
              <div>
                <span className="text-gray-600">Mô tả:</span>
                <p className="ml-2 text-gray-700">{myCompany.description}</p>
              </div>
            )}
            {myCompany.website && (
              <div>
                <span className="text-gray-600">Website:</span>
                <a
                  href={myCompany.website}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="ml-2 text-blue-600 hover:underline"
                >
                  {myCompany.website}
                </a>
              </div>
            )}
            {myCompany.email && (
              <div>
                <span className="text-gray-600">Email:</span>
                <span className="ml-2">{myCompany.email}</span>
              </div>
            )}
            {myCompany.phone && (
              <div>
                <span className="text-gray-600">Điện thoại:</span>
                <span className="ml-2">{myCompany.phone}</span>
              </div>
            )}
            {myCompany.address && (
              <div>
                <span className="text-gray-600">Địa chỉ:</span>
                <span className="ml-2">{myCompany.address}</span>
              </div>
            )}
          </div>
        </Card>
      )}
    </div>
  );
}




