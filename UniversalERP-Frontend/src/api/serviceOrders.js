import { httpGet, httpPost, httpPatch, httpDelete } from "./http";

// Sistemdeki tüm teknik servis iş emirlerinin listesini getiren fonksiyon
export async function getServiceOrders() {
  return httpGet("/api/serviceorders");
}

// Belirtilen benzersiz kimlik (ID) değerine sahip tek bir servis iş emrinin detayını getiren fonksiyon
export async function getServiceOrder(id) {
  return httpGet(`/api/serviceorders/${id}`);
}

// Yeni bir teknik servis iş emri (arıza/tamir kaydı) oluşturan fonksiyon
// Gönderilecek Nesne (Body): { deviceName, customerId, customerName, issueDescription, serviceFee, partCost, assignedUserId }
export async function createServiceOrder(dto) {
  return httpPost("/api/serviceorders", dto);
}

// İş emrinin güncel durumunu (aşamasını) değiştiren kısmi güncelleme fonksiyonu (PATCH)
// Gönderilecek Nesne (Body): { status }
// Alabileceği Değerler: "Beklemede" | "Tamirde" | "Tamamlandı" | "Teslim Edildi" | "İptal"
export async function updateServiceOrderStatus(id, status) {
  return httpPatch(`/api/serviceorders/${id}/status`, { status });
}

// Belirtilen kimlik (ID) değerine sahip servis iş emrini sistemden silen fonksiyon
export async function deleteServiceOrder(id) {
  return httpDelete(`/api/serviceorders/${id}`);
}