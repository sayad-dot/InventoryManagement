-- Simple SQL script to make sayadkhan0555@gmail.com an admin
-- Run this in your PostgreSQL database

-- 1. Ensure Admin role exists
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
SELECT gen_random_uuid()::text, 'Admin', 'ADMIN', gen_random_uuid()::text
WHERE NOT EXISTS (SELECT 1 FROM "AspNetRoles" WHERE "Name" = 'Admin');

-- 2. Make the user admin (replace email if needed)
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM "AspNetUsers" u
CROSS JOIN "AspNetRoles" r
WHERE u."Email" = 'sayadkhan0555@gmail.com'
AND r."Name" = 'Admin'
AND NOT EXISTS (
    SELECT 1 FROM "AspNetUserRoles" ur
    WHERE ur."UserId" = u."Id" AND ur."RoleId" = r."Id"
);

-- 3. Verify the user is now admin
SELECT u."Email", u."FullName", r."Name" as "Role"
FROM "AspNetUsers" u
INNER JOIN "AspNetUserRoles" ur ON u."Id" = ur."UserId"
INNER JOIN "AspNetRoles" r ON ur."RoleId" = r."Id"
WHERE u."Email" = 'sayadkhan0555@gmail.com';