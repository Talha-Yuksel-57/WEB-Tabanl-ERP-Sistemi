import { httpGet, httpPut } from "./http";

// Kayıt formunda kullanıcıların üye olacakları firmayı seçebilmeleri için tüm kiracıları (firmaları) listeleyen fonksiyon
// Not: Henüz sisteme giriş yapmamış kullanıcılar erişeceği için backend'de 'AllowAnonymous' olarak yapılandırılmıştır.
// Bu yüzden istek atılırken '{ auth: false }' parametresi geçilerek JWT Token zorunluluğu devre dışı bırakılır.
export async function getTenants() {
  return httpGet("/api/tenants", { auth: false });
}

// Oturumu açık olan mevcut firmanın sistem/şirket genel ayarlarını getiren fonksiyon
export async function getTenantSettings() {
  return httpGet("/api/tenants/settings");
}

// Mevcut firmanın şirket bilgilerini (firma adı, adres, vergi numarası, sektör vb.) güncelleyen fonksiyon
// Gönderilecek Nesne (Body): { name, address, taxNumber, industry }
export async function updateTenantSettings(dto) {
  return httpPut("/api/tenants/settings", dto);
}