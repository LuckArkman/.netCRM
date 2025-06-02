using Entities;
using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Entities;

namespace GabineteDigital.Api
{
    public static class SeedData
    {
        public static async Task Initialize(GabineteDigitalDbContext context,
                                            UserManager<MembroEquipe> userManager,
                                            RoleManager<IdentityRole<Guid>> roleManager)
        {
            await context.Database.MigrateAsync(); // Aplica migrações pendentes

            // Criar Roles (perfis de usuário)
            string[] roleNames = { "Admin", "Secretario", "Assessor", "Atendente" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }

            // Criar usuário Admin padrão
            if (await userManager.FindByEmailAsync("admin@gabinete.com") == null)
            {
                var adminUser = new MembroEquipe
                {
                    NomeCompleto = "Administrador Geral",
                    UserName = "admin@gabinete.com",
                    Email = "admin@gabinete.com",
                    EmailConfirmed = true,
                    Cargo = "Administrador",
                    PhoneNumber = "999999999",
                    Ativo = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Senha temporária, mude em produção!
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Criar usuário Secretário padrão
            if (await userManager.FindByEmailAsync("secretario@gabinete.com") == null)
            {
                var secretarioUser = new MembroEquipe
                {
                    NomeCompleto = "Maria Secretária",
                    UserName = "secretario@gabinete.com",
                    Email = "secretario@gabinete.com",
                    EmailConfirmed = true,
                    Cargo = "Secretário",
                    PhoneNumber = "888888888",
                    Ativo = true
                };
                var result = await userManager.CreateAsync(secretarioUser, "Secret@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(secretarioUser, "Secretario");
                }
            }

            // Exemplos de Projetos do Vereador (para a IA)
            if (!context.ProjetosVereador.Any())
            {
                context.ProjetosVereador.AddRange(
                    new ProjetoVereador
                    {
                        Id = Guid.NewGuid(),
                        Titulo = "Requalificação da Praça Central",
                        Descricao = "Projeto para revitalização e modernização da Praça Central, incluindo nova iluminação, paisagismo e áreas de lazer para a comunidade.",
                        StatusAtual = "Em discussão na Câmara",
                        Area = "Urbanismo",
                        DataCriacao = DateTime.Now.AddMonths(-3),
                        DataUltimaAtualizacao = DateTime.Now.AddDays(-15)
                    },
                    new ProjetoVereador
                    {
                        Id = Guid.NewGuid(),
                        Titulo = "Programa 'Escola Conectada'",
                        Descricao = "Iniciativa para levar internet de alta velocidade e equipamentos digitais a todas as escolas públicas municipais, visando a inclusão digital.",
                        StatusAtual = "Aguardando dotação orçamentária",
                        Area = "Educação",
                        DataCriacao = DateTime.Now.AddMonths(-6),
                        DataUltimaAtualizacao = DateTime.Now.AddDays(-30)
                    },
                    new ProjetoVereador
                    {
                        Id = Guid.NewGuid(),
                        Titulo = "Aumento da Frota de Transporte Público",
                        Descricao = "Proposta para aquisição de novos ônibus e ampliação de linhas para otimizar o transporte público na cidade, melhorando a mobilidade urbana.",
                        StatusAtual = "Em fase de audiências públicas",
                        Area = "Transporte Público",
                        DataCriacao = DateTime.Now.AddMonths(-1),
                        DataUltimaAtualizacao = DateTime.Now.AddDays(-5)
                    }
                );
                await context.SaveChangesAsync();
            }

            // Adicione mais dados de seed para testes se necessário
            if (!context.Cidadaos.Any())
            {
                context.Cidadaos.AddRange(
                    new Cidadao { Id = Guid.NewGuid(), Nome = "João Silva", Email = "joao.silva@email.com", Telefone = "11911111111", Endereco = "Rua A, 123", Bairro = "Centro", CPF = "111.111.111-11" },
                    new Cidadao { Id = Guid.NewGuid(), Nome = "Maria Oliveira", Email = "maria.o@email.com", Telefone = "11922222222", Endereco = "Av. B, 456", Bairro = "Bairro Novo", CPF = "222.222.222-22" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}