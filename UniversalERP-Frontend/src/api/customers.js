// HTTP isteklerinde kullanılacak ortak fonksiyonlar içe aktarılır.
import { httpGet, httpPost, httpPut, httpDelete } from "./http";

/**
 * Sistemde kayıtlı tüm müşterileri getirir.
 */
export async function getCustomers() {
  return httpGet("/api/customers");
}

/**
 * Belirtilen ID'ye sahip müşterinin bilgilerini getirir.
 */
export async function getCustomer(id) {
  return httpGet(`/api/customers/${id}`);
}

/**
 * Yeni bir müşteri kaydı oluşturur.
 * dto nesnesi; ad, e-posta, telefon ve adres bilgilerini içerir.
 */
export async function createCustomer(dto) {
  return httpPost("/api/customers", dto);
}

/**
 * Mevcut müşterinin bilgilerini günceller.
 */
export async function updateCustomer(id, dto) {
  return httpPut(`/api/customers/${id}`, dto);
}

/**
 * Belirtilen ID'ye sahip müşteriyi sistemden siler.
 */
export async function deleteCustomer(id) {
  return httpDelete(`/api/customers/${id}`);
}