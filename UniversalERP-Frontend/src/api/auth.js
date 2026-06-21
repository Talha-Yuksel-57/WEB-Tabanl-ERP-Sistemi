// HTTP POST isteklerini gerçekleştiren ortak fonksiyon içe aktarılır.
import { httpPost } from "./http";

/**
 * Kullanıcının sisteme giriş yapmasını sağlar.
 * Backend'e e-posta ve şifre bilgileri gönderilir.
 */
export async function login(email, password) {
  return httpPost("/api/auth/login", { email, password }, { auth: false });
}

/**
 * Sisteme yeni kullanıcı kaydı oluşturur.
 * Kullanıcının temel bilgileri backend'e gönderilir.
 */
export async function register({ email, password, fullName, tenantId, role }) {
  return httpPost(
    "/api/auth/register",
    { email, password, fullName, tenantId, role },
    { auth: false }
  );
}

/**
 * Süresi dolan erişim belirtecini (Access Token)
 * yenilemek için Refresh Token kullanır.
 */
export async function refreshToken(refreshTokenValue) {
  return httpPost(
    "/api/auth/refresh",
    { refreshToken: refreshTokenValue },
    { auth: false }
  );
}

/**
 * Kullanıcının sistemden güvenli şekilde çıkış yapmasını sağlar.
 */
export async function logout() {
  return httpPost("/api/auth/logout", {});
}

/**
 * JWT içerisindeki payload bilgisini çözümler
 * ve JSON nesnesi olarak döndürür.
 */
export function decodeJwt(token) {
  try {
    const payload = token.split(".")[1];
    const json = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
    return JSON.parse(decodeURIComponent(escape(json)));
  } catch {
    // Token geçersiz ise null döndürülür.
    return null;
  }
}