import { useEffect, useState } from "react";
import Input from "../../components/ui/input";
import Button from "../../components/ui/button";
import { Link, useNavigate } from "react-router-dom";
import apiService from "../../lib/api.service";
import { jwtDecode } from "jwt-decode";
import { GoogleLogin } from "@react-oauth/google";
import { toast } from "sonner";
import { useNotificationStore } from "../../stores/notificationStore";
import { useI18n } from "../../hooks/useI18n";

export default function Login() {
  const nav = useNavigate();
  const { t } = useI18n();

  // ✅ Thêm rememberMe trong state form
  const [form, setForm] = useState({
    email: "",
    password: "",
    rememberMe: false,
  });
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(false);

  const [showForgot, setShowForgot] = useState(false);
  const [step, setStep] = useState(1); // 1: nhập email, 2: nhập code + mật khẩu mới
  const [email, setEmail] = useState("");
  const [code, setCode] = useState("");
  const [newPassword, setNewPassword] = useState("");

  const onSubmit = async (e) => {
    e.preventDefault();
    if (!form.email || !form.password)
      return setErr(t("Common.PleaseEnterInfo"));

    setErr("");
    setLoading(true);
    try {
      // ✅ Gửi rememberMe lên server
      const response = await apiService.post("/auth/login", {
        email: form.email,
        password: form.password,
        rememberMe: form.rememberMe,
      });

      // apiService đã extract data từ ApiResponse, response là { accessToken, expiresIn }
      const token = response?.accessToken || response?.token;
      if (!token) {
        console.error("Login response:", response);
        throw new Error(t("Auth.NoTokenFromServer"));
      }

      // ✅ Lưu token theo lựa chọn Remember Me
      if (form.rememberMe) {
        localStorage.setItem("token", token);
      } else {
        sessionStorage.setItem("token", token);
      }

      // Token sẽ được tự động gắn bởi api interceptor

      // Decode JWT để lấy thông tin user
      const decoded = jwtDecode(token);

      console.log("=== DECODED JWT ===");
      console.log(JSON.stringify(decoded, null, 2));

      const userId =
        decoded.sub ||
        decoded[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
        ] ||
        decoded[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier".toLowerCase()
        ] ||
        decoded.userId ||
        null;

      const email =
        decoded.email ||
        decoded[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
        ] ||
        decoded[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress".toLowerCase()
        ] ||
        form.email;

      const role =
        decoded[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ] ||
        decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role"] ||
        decoded.role ||
        decoded.role?.toString() ||
        "User";

      const userData = {
        id: userId,
        email: email,
        role: role,
      };

      // ✅ Lưu user theo rememberMe
      if (form.rememberMe) {
        localStorage.setItem("user", JSON.stringify(userData));
      } else {
        sessionStorage.setItem("user", JSON.stringify(userData));
      }

      console.log("UserData saved:", userData);
      const notifStore = useNotificationStore.getState();
      await notifStore.reset(); // 🧹 Dọn store và stop connection cũ nếu có
      await notifStore.fetchFromServer(); // 📥 Load lại thông báo trong DB
      await notifStore.initConnection(); // 🔗 Mở kết nối SignalR bằng token mới
      nav("/", { replace: true });
    } catch (error) {
      console.error("Login error:", error);
      const msg =
        error?.response?.data?.message ||
        error?.message ||
        t("Auth.LoginFailed");
      setErr(msg);
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleSuccess = async (credentialResponse) => {
    // Check if credentialResponse is valid
    if (!credentialResponse?.credential) {
      toast.error(t("Auth.NoTokenFromGoogle"));
      return;
    }
    try {
      const idToken = credentialResponse.credential;
      const response = await apiService.post("/auth/google", { idToken });

      // apiService đã extract data từ ApiResponse, response là { accessToken, expiresIn }
      const token = response?.accessToken || response?.token;
      if (!token) {
        console.error("Google login response:", response);
        throw new Error("Google login failed: No token");
      }

      localStorage.setItem("token", token);

      const decoded = jwtDecode(token);
      console.log("Decoded JWT (Google):", decoded);

      const userId =
        decoded.sub ||
        decoded[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
        ] ||
        decoded.userId;

      const email =
        decoded[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
        ] || decoded.email;

      const role =
        decoded[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ] ||
        decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role"] ||
        decoded["role"] ||
        decoded.role ||
        "User";

      const userData = {
        id: userId,
        email: email,
        role: role,
      };

      localStorage.setItem("user", JSON.stringify(userData));

      const notifStore = useNotificationStore.getState();
      await notifStore.reset(); // 🧹 Clear store & stop old connection
      await notifStore.fetchFromServer(); // 📥 Load noti mới theo user
      await notifStore.initConnection(); // 🔗 Kết nối SignalR bằng token mới
      console.log("UserData saved (Google):", userData);

      toast.success(t("Auth.LoginSuccess"));
      nav("/", { replace: true });
    } catch (err) {
      console.error("Google login error:", err);
      toast.error(err?.message || err?.response?.data?.message || t("Auth.LoginFailed"));
    }
  };

  const handleSendCode = async () => {
    if (!email) return toast.error(t("Common.PleaseEnterInfo"));
    try {
      await apiService.post("/auth/forgot-password", { email });
      toast.success(t("Auth.SendCodeSuccess"));
      setStep(2);
    } catch (err) {
      toast.error(err?.message || err?.response?.data?.message || t("Auth.CannotSendCode"));
    }
  };

  const handleResetPassword = async () => {
    if (!code || !newPassword)
      return toast.error(t("Common.PleaseEnterFullInfo"));
    try {
      await apiService.post("/auth/reset-password", { email, code, newPassword });
      toast.success(t("Auth.ResetPasswordSuccess"));
      setShowForgot(false);
      setStep(1);
      setEmail("");
      setCode("");
      setNewPassword("");
    } catch (err) {
      toast.error(err.response?.data?.message || "Đặt lại mật khẩu thất bại.");
    }
  };

  return (
    <div className="container-ld py-12 grid md:grid-cols-2 gap-10">
      <div className="hidden md:block">
        <div className="h-80 rounded-2xl bg-gradient-to-br from-blue-200 to-orange-200" />
        <h2 className="mt-6 text-2xl font-semibold">
          {t("Auth.WelcomeBack")}
        </h2>
        <p className="text-slate-600 mt-2">
          {t("Auth.LoginDescription")}
        </p>
      </div>

      <div className="card">
        <form className="card-body space-y-4" onSubmit={onSubmit}>
          <h1 className="text-2xl font-semibold">{t("Auth.Login")}</h1>
          {err && <div className="text-sm text-red-600">{err}</div>}
          <div>
            <label className="text-sm">{t("Auth.Email")}</label>
            <Input
              type="email"
              placeholder="you@example.com"
              value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })}
            />
          </div>
          <div>
            <label className="text-sm">{t("Auth.Password")}</label>
            <Input
              type="password"
              placeholder="••••••••"
              value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
            />
          </div>

          <div className="flex items-center justify-between text-sm">
            <label className="flex items-center gap-2">
              {/* ✅ Checkbox Remember Me */}
              <input
                type="checkbox"
                checked={form.rememberMe}
                onChange={(e) =>
                  setForm({ ...form, rememberMe: e.target.checked })
                }
              />
              {t("Auth.RememberMe")}
            </label>
            <button
              type="button"
              onClick={() => setShowForgot(true)}
              className="text-brand-700 hover:underline"
            >
              {t("Auth.ForgotPassword")}
            </button>
          </div>

          <Button type="submit" className="w-full" disabled={loading}>
            {loading ? t("Auth.LoggingIn") : t("Auth.Login")}
          </Button>

          <div className="flex justify-center mt-4">
            <GoogleLogin
              onSuccess={handleGoogleSuccess}
              onError={(error) => {
                console.warn("Google Login error (may be due to origin not configured):", error);
                // Only show toast if it's not a configuration error
                if (error?.error !== "popup_closed_by_user") {
                  toast.error(t("Auth.GoogleLoginUnavailable"));
                }
              }}
            />
          </div>

          <div className="text-sm text-center text-slate-600">
            {t("Auth.NoAccount")}{" "}
            <Link to="/register" className="text-brand-700 hover:underline">
              {t("Auth.Register")}
            </Link>
          </div>
        </form>
      </div>

      {showForgot && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 shadow-lg w-full max-w-md space-y-4">
            <h2 className="text-xl font-semibold text-center">
              {step === 1 ? t("Auth.ForgotPasswordTitle") : t("Auth.ResetPasswordTitle")}
            </h2>

            {step === 1 ? (
              <>
                <p className="text-sm text-slate-600">
                  {t("Auth.ForgotPasswordDescription")}
                </p>
                <Input
                  placeholder="you@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                />
                <div className="flex justify-end gap-2 mt-4">
                  <Button
                    variant="outline"
                    onClick={() => setShowForgot(false)}
                  >
                    {t("Common.Cancel")}
                  </Button>
                  <Button onClick={handleSendCode}>{t("Auth.SendCode")}</Button>
                </div>
              </>
            ) : (
              <>
                <Input
                  placeholder={t("Auth.VerificationCode")}
                  value={code}
                  onChange={(e) => setCode(e.target.value)}
                />
                <Input
                  type="password"
                  placeholder={t("Auth.NewPassword")}
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                />
                <div className="flex justify-end gap-2 mt-4">
                  <Button
                    variant="outline"
                    onClick={() => setShowForgot(false)}
                  >
                    {t("Common.Cancel")}
                  </Button>
                  <Button onClick={handleResetPassword}>{t("Auth.ChangePassword")}</Button>
                </div>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
