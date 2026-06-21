import { httpGet, httpPut, httpPost } from "./http";

// Oturumu açık olan kullanıcının profil bilgilerini getiren fonksiyon
export async function getProfile() {
  return httpGet("/api/profile");
}

// Kullanıcının profil bilgilerini (ad-soyad, departman vb.) güncelleyen fonksiyon
// Gönderilecek Nesne (Body): { fullName, department }
export async function updateProfile(dto) {
  return httpPut("/api/profile", dto);
}

// Kullanıcının mevcut şifresini yenisiyle değiştiren fonksiyon
// Gönderilecek Nesne (Body): { currentPassword, newPassword }
export async function changePassword(dto) {
  return httpPost("/api/profile/change-password", dto);
}