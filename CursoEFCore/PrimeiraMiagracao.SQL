﻿CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "Cliente" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "Nome" VARCHAR(80) NOT NULL,
    "Telefone" CHAR(11) NOT NULL,
    "CEP" CHAR(8) NOT NULL,
    "Estado" CHAR(2) NOT NULL,
    "Cidade" character varying(60) NOT NULL,
    CONSTRAINT "PK_Cliente" PRIMARY KEY ("Id")
);

CREATE TABLE "Produto" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "CodigoBarras" VARCHAR(14) NOT NULL,
    "Descricao" VARCHAR(60) NULL,
    "Valor" numeric NOT NULL,
    "TipoProduto" text NOT NULL,
    "Ativo" boolean NOT NULL,
    CONSTRAINT "PK_Produto" PRIMARY KEY ("Id")
);

CREATE TABLE "Pedido" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "ClienteId" integer NOT NULL,
    "IniciadoEm" timestamp without time zone NOT NULL DEFAULT (current_date),
    "Finalizado" timestamp without time zone NOT NULL,
    "TipoFrete" integer NOT NULL,
    "Status" text NOT NULL,
    "Observacao" VARCHAR(512) NULL,
    CONSTRAINT "PK_Pedido" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Pedido_Cliente_ClienteId" FOREIGN KEY ("ClienteId") REFERENCES "Cliente" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PedidoItem" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "PedidoId" integer NOT NULL,
    "ProdutoId" integer NOT NULL,
    "Quantidade" integer NOT NULL DEFAULT 0,
    "Valor" numeric NOT NULL DEFAULT 0.0,
    "Desconto" numeric NOT NULL DEFAULT 0.0,
    CONSTRAINT "PK_PedidoItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PedidoItem_Pedido_PedidoId" FOREIGN KEY ("PedidoId") REFERENCES "Pedido" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PedidoItem_Produto_ProdutoId" FOREIGN KEY ("ProdutoId") REFERENCES "Produto" ("Id") ON DELETE CASCADE
);

CREATE INDEX idx_cliente_telefone ON "Cliente" ("Telefone");

CREATE INDEX "IX_Pedido_ClienteId" ON "Pedido" ("ClienteId");

CREATE INDEX "IX_PedidoItem_PedidoId" ON "PedidoItem" ("PedidoId");

CREATE INDEX "IX_PedidoItem_ProdutoId" ON "PedidoItem" ("ProdutoId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210126172107_PrimeiraMigration', '5.0.2');

COMMIT;

