// API'nin temel adresi .env dosyasından alınır.
// Tanımlı değilse varsayılan olarak localhost kullanılır.
const BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5240";

/**
 * Local Storage'da saklanan JWT erişim belirtecini döndürür.
 */
function getToken() {
  return localStorage.getItem("erp_token") || "";
}

/**
 * JWT içerisindeki TenantId bilgisini alır.
 * Multi-tenant yapıda her isteğin doğru şirkete yönlendirilmesini sağlar.
 */
function getTenantId() {
  const token = getToken();
  if (!token) return null;

  try {
    const payload = JSON.parse(atob(token.split(".")[1]));

    // TenantId farklı isimlerle gelebileceği için her iki alan da kontrol edilir.
    const tid = payload["TenantId"] ?? payload["tenantId"] ?? null;

    return tid ? String(tid) : null;
  } catch {
    return null;
  }
}

/**
 * HTTP isteklerinde kullanılacak başlık (Header) bilgilerini oluşturur.
 */
function buildHeaders({ json = true, auth = true } = {}) {
  const headers = {};

  // JSON veri gönderilecekse Content-Type eklenir.
  if (json) headers["Content-Type"] = "application/json";

  if (auth) {
    const token = getToken();

    // Kullanıcı giriş yaptıysa Authorization başlığı eklenir.
    if (token) headers["Authorization"] = `Bearer ${token}`;

    // Multi-tenant desteği için TenantId header'a eklenir.
    const tenantId = getTenantId();
    if (tenantId) headers["X-Tenant-Id"] = tenantId;
  }

  return headers;
}

/**
 * API'den dönen yanıtı işler.
 * Başarısız isteklerde hata mesajını yakalayarak hata oluşturur.
 */
async function handle(res) {
  const text = await res.text();

  let data = null;

  try {
    data = text ? JSON.parse(text) : null;
  } catch {
    data = text;
  }

  if (!res.ok) {
    const msg =
      (data && (data.message || data.Message)) ||
      (typeof data === "string" ? data : null) ||
      `HTTP ${res.status}`;

    const err = new Error(msg);
    err.status = res.status;
    err.data = data;

    throw err;
  }

  return data;
}

/**
 * GET isteği gönderir.
 */
export async function httpGet(path, opts) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: "GET",
    headers: buildHeaders(opts),
  });

  return handle(res);
}

/**
 * POST isteği gönderir.
 */
export async function httpPost(path, body, opts) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: "POST",
    headers: buildHeaders(opts),
    body: JSON.stringify(body ?? {}),
  });

  return handle(res);
}

/**
 * PUT isteği gönderir.
 */
export async function httpPut(path, body, opts) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: "PUT",
    headers: buildHeaders(opts),
    body: JSON.stringify(body ?? {}),
  });

  return handle(res);
}

/**
 * PATCH isteği gönderir.
 */
export async function httpPatch(path, body, opts) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: "PATCH",
    headers: buildHeaders(opts),
    body: JSON.stringify(body ?? {}),
  });

  return handle(res);
}

/**
 * DELETE isteği gönderir.
 */
export async function httpDelete(path, opts) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: "DELETE",
    headers: buildHeaders(opts),
  });

  return handle(res);
}

// API adresi gerektiğinde diğer dosyalarda kullanılmak üzere dışa aktarılır.
export { BASE_URL };