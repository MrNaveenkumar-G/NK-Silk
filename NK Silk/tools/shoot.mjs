import { chromium } from 'playwright';
import { mkdirSync } from 'fs';

const BASE = 'http://localhost:5035';
const OUT = 'D:/Naveen/NK Silk/docs/screenshots';
mkdirSync(OUT, { recursive: true });

async function login(ctx, email, password) {
  const page = await ctx.newPage();
  await page.goto(`${BASE}/Account/Login`, { waitUntil: 'networkidle' });
  await page.fill('#Email', email);
  await page.fill('#Password', password);
  await Promise.all([page.waitForLoadState('networkidle'), page.click('button[type="submit"]')]);
  await page.close();
}

async function shoot(ctx, path, file) {
  const page = await ctx.newPage();
  const resp = await page.goto(`${BASE}${path}`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(300);
  await page.screenshot({ path: `${OUT}/${file}.png`, fullPage: true });
  console.log(`${file.padEnd(28)} ${path}  ->  ${resp.status()}`);
  await page.close();
}

const browser = await chromium.launch();

// ---- Admin session ----
const admin = await browser.newContext({ viewport: { width: 1280, height: 900 } });
await login(admin, 'admin@nksilk.com', 'Admin@123');
await shoot(admin, '/Admin/Reports?days=30', '01-admin-reports');
await shoot(admin, '/Admin/Shipments', '02-admin-shipments-list');
await shoot(admin, '/Admin/Shipments/Manage/NK20260611130056678', '03-admin-shipment-manage');
await shoot(admin, '/Admin/Support', '04-admin-support-queue');
// open the first ticket in the queue, if any
{
  const page = await admin.newPage();
  await page.goto(`${BASE}/Admin/Support`, { waitUntil: 'networkidle' });
  const link = await page.locator('a:has-text("Open")').first();
  if (await link.count()) {
    await Promise.all([page.waitForLoadState('networkidle'), link.click()]);
    await page.screenshot({ path: `${OUT}/05-admin-support-ticket.png`, fullPage: true });
    console.log('05-admin-support-ticket       (opened first ticket)');
  }
  await page.close();
}
// customer-facing support (admin is also a customer)
await shoot(admin, '/Support', '06-customer-support-list');
await shoot(admin, '/Support/Create', '07-customer-support-create');
await admin.close();

// ---- Vendor session ----
const seller = await browser.newContext({ viewport: { width: 1280, height: 900 } });
await login(seller, 'seller@nksilk.com', 'Seller@123');
await shoot(seller, '/Vendor/Dashboard', '08-vendor-dashboard');
await shoot(seller, '/Vendor/Products', '09-vendor-products');
await shoot(seller, '/Vendor/Inventory', '10-vendor-inventory');
await shoot(seller, '/Vendor/Orders', '11-vendor-orders');
await seller.close();

await browser.close();
console.log('done');
