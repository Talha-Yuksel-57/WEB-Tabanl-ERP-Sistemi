import { httpGet, httpPost, httpPut, httpDelete } from "./http";

// Sistemdeki veya projeye ait tüm iş görevlerini (tasks) listeleyen fonksiyon
export async function getProjectTasks() {
  return httpGet("/api/projecttasks");
}

// Belirtilen benzersiz kimlik (ID) değerine sahip tek bir görevin detayını getiren fonksiyon
export async function getProjectTask(id) {
  return httpGet(`/api/projecttasks/${id}`);
}

// Yeni bir proje iş görevi oluşturan fonksiyon
// Gönderilecek Nesne (Body): { taskTitle, description, projectId, assignedDeveloperId, priority }
export async function createProjectTask(dto) {
  return httpPost("/api/projecttasks", dto);
}

// Mevcut bir iş görevinin detaylarını ve ilerleme durumunu güncelleyen fonksiyon
// Gönderilecek Nesne (Body): { taskTitle, description, status, priority, hoursWorked }
export async function updateProjectTask(id, dto) {
  return httpPut(`/api/projecttasks/${id}`, dto);
}

// Belirtilen kimlik (ID) değerine sahip iş görevini sistemden silen fonksiyon
export async function deleteProjectTask(id) {
  return httpDelete(`/api/projecttasks/${id}`);
}