// Backend'deki [Authorize(Roles="...")] kurallarıyla birebir eşleşir.
// Frontend bu kontrolleri sadece UX için yapar (buton gizleme);
// gerçek güvenlik backend'de zaten sağlanıyor.

export const CAN_MANAGE_PRODUCTS = ["SuperAdmin", "TenantAdmin", "Manager"];
export const CAN_DELETE_PRODUCTS = ["SuperAdmin", "TenantAdmin"];
export const CAN_UPDATE_STOCK = ["SuperAdmin", "TenantAdmin", "Manager", "Cashier"];
export const CAN_DELETE_CUSTOMERS = ["SuperAdmin", "TenantAdmin"];

export const CAN_CREATE_SALE = ["SuperAdmin", "TenantAdmin", "Manager", "Cashier"];
export const CAN_CANCEL_SALE = ["SuperAdmin", "TenantAdmin", "Manager"];

export const CAN_CREATE_SERVICE_ORDER = ["SuperAdmin", "TenantAdmin", "Manager", "Technician"];
export const CAN_UPDATE_SERVICE_STATUS = ["SuperAdmin", "TenantAdmin", "Manager", "Technician"];
export const CAN_DELETE_SERVICE_ORDER = ["SuperAdmin", "TenantAdmin"];

export const CAN_CREATE_TASK = ["SuperAdmin", "TenantAdmin", "Manager"];
export const CAN_UPDATE_TASK = ["SuperAdmin", "TenantAdmin", "Manager", "Employee"];
export const CAN_DELETE_TASK = ["SuperAdmin", "TenantAdmin", "Manager"];

export const CAN_VIEW_REPORTS = ["SuperAdmin", "TenantAdmin", "Manager"];
export const CAN_VIEW_AUDIT_LOGS = ["SuperAdmin", "TenantAdmin"];
export const CAN_EDIT_TENANT_SETTINGS = ["SuperAdmin", "TenantAdmin"];

export function hasRole(role, allowedRoles) {
  return allowedRoles.includes(role);
}
