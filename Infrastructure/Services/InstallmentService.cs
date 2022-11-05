﻿using Domain.Entities;
using Domain.Dtos;
using Infrastructure.Context;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class InstallmentService
{
    private readonly DataContext _context;
    public InstallmentService(DataContext context)
    {
        _context = context;
    }

    public async Task<AddInstallmentDto> AddInstallment(AddInstallmentDto installmentDto)
    {
        var installment = new Installment
        {
            EndInstallment = installmentDto.EndInstallment,
            Percentage = installmentDto.Percentage,
            ProductId = installmentDto.ProductId,
        };

        await _context.Installments.AddAsync(installment);
        await _context.SaveChangesAsync();

        installmentDto.Id = installment.Id;


        var installmentCreated = await GetInstallmentById(installment.Id);
        return installmentCreated;
    }

    public async Task<bool> DeleteInstallment(int id)
    {
        var installment = await _context.Installments.FirstOrDefaultAsync(e => e.Id == id);

        if (installment == null)
        {
            return false;
        }

        _context.Installments.Remove(installment);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<GetInstallmentDto>> GetInstallments()
    {
        var installments = await
        (
            from ins in _context.Installments
            join pr in _context.Products
            on ins.ProductId equals pr.Id
            select new GetInstallmentDto()
            {
                Id = ins.Id,
                EndInstallment = ins.EndInstallment,
                Percentage = ins.Percentage,
                ProductName = pr.ProductName,
                Orders =
                                          (
                                              from or in _context.Orders
                                              where ins.Id == or.InstallmentId
                                              select new GetOrderDto()
                                              {
                                                  Id = or.Id,
                                                  InstallmentId = ins.Id,
                                                  InstallmentName = ins.EndInstallment,
                                              }
                                              ).ToList()
            }).ToListAsync();
        return installments;
    }

    public async Task<AddInstallmentDto> GetInstallmentById(int id)
    {
        var Installment = await _context.Installments
            .Select(cu => new AddInstallmentDto()
            {
                Id = cu.Id,
                InstallmentName = cu.InstallmentName,
                PhoneNumber = cu.PhoneNumber,
            }).FirstOrDefaultAsync(tu => tu.Id == id);
        return Installment;
    }

    public async Task<AddInstallmentDto> UpdateInstallment(AddInstallmentDto InstallmentDto)
    {

        var Installment = await _context.Installments.FirstOrDefaultAsync(pa => pa.Id == InstallmentDto.Id);

        if (Installment == null)
        {
            return null;
        }

        Installment.InstallmentName = InstallmentDto.InstallmentName;
        Installment.PhoneNumber = InstallmentDto.PhoneNumber;

        await _context.SaveChangesAsync();

        var InstallmentUpdated = await GetInstallmentById(Installment.Id);
        return InstallmentUpdated;
    }
}