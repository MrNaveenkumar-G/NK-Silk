/* ============================================================================
   NK Silk - Seed Data : RUN ALL
   ----------------------------------------------------------------------------
   Runs every seed script in dependency order.

   HOW TO RUN (SSMS): enable SQLCMD Mode first
       Query menu  ->  SQLCMD Mode
   then execute this file. The :r includes are resolved relative to THIS file's
   folder, so keep all scripts together in "Insert Scripts".

   Prerequisite: the schema must already exist - run, from the parent folder:
       ..\01_Tables.sql   then (optionally)   ..\02_Views.sql

   Every script is idempotent, so re-running is safe.
   ============================================================================ */
:setvar SeedDir "."
PRINT '=== NK Silk seed: starting ===';
GO
:r $(SeedDir)\01_Categories.sql
:r $(SeedDir)\02_Colors.sql
:r $(SeedDir)\03_Sizes.sql
:r $(SeedDir)\04_Roles.sql
:r $(SeedDir)\05_Coupons.sql
:r $(SeedDir)\06_Vendors.sql
:r $(SeedDir)\07_Customers.sql
:r $(SeedDir)\08_CustomerRoles.sql
:r $(SeedDir)\09_Products.sql
:r $(SeedDir)\10_VendorProductAssignment.sql
:r $(SeedDir)\11_Offers.sql
:r $(SeedDir)\12_ComboPacks.sql
GO
PRINT '=== NK Silk seed: complete ===';
GO
