// HTTP GET isteklerini gerçekleştiren ortak fonksiyon içe aktarılır.
// Bu fonksiyon, backend API'lerine GET isteği göndermek için kullanılır.
import { httpGet } from "./http";

/*
|--------------------------------------------------------------------------
| Audit Log Servisi
|--------------------------------------------------------------------------
|
| Bu dosya, sistemde tutulan denetim (Audit Log) kayıtlarını backend
| servisinden almak için kullanılan fonksiyonları içerir.
|
| Backend Endpoint:
| GET /api/auditlogs
|
| Desteklenen filtre parametreleri:
| - entityName : İşlem yapılan tablo veya varlık adı
| - action     : Gerçekleştirilen işlem türü (Create, Update, Delete vb.)
| - from       : Başlangıç tarihi
| - to         : Bitiş tarihi
| - page       : Sayfa numarası
| - pageSize   : Her sayfada gösterilecek kayıt sayısı
|
| Örnek dönüş değeri:
| {
|   total: 120,
|   page: 1,
|   pageSize: 10,
|   data: [ ... ]
| }
|--------------------------------------------------------------------------
*/

/**
 * Audit Log kayıtlarını backend API üzerinden getirir.
 *
 * @param {Object} filter
 * Filtreleme amacıyla kullanılan parametreleri içerir.
 *
 * Örnek:
 * {
 *   entityName: "Product",
 *   action: "Update",
 *   from: "2025-01-01",
 *   to: "2025-01-31",
 *   page: 1,
 *   pageSize: 10
 * }
 *
 * @returns {Promise<Object>}
 * Backend tarafından döndürülen audit log listesini döndürür.
 */
export async function getAuditLogs(filter = {}) {

  // Filtre nesnesindeki boş (undefined, null veya "")
  // değerleri kaldırır.
  // Böylece gereksiz sorgu parametreleri URL'e eklenmez.
  const params = Object.entries(filter).filter(
    ([, v]) => v !== undefined && v !== null && v !== ""
  );

  // Filtre parametreleri mevcutsa Query String oluşturur.
  //
  // Örnek:
  // ?entityName=Product&page=1&pageSize=10
  //
  // Parametre bulunmuyorsa boş string döndürür.
  const qs = params.length
    ? `?${new URLSearchParams(params).toString()}`
    : "";

  // Oluşturulan URL'e GET isteği gönderir.
  // Backend'den dönen Promise nesnesini çağıran metoda iletir.
  return httpGet(`/api/auditlogs${qs}`);
}