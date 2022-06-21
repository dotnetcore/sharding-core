START TRANSACTION;

CREATE TABLE "TestModelKeys" (
    "Id" uuid NOT NULL,
    "Key" character varying(36) NOT NULL,
    "CreationDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_TestModelKeys" PRIMARY KEY ("Id", "Key")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220616075026_TestModelKey_Create', '6.0.5');

COMMIT;

