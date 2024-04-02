﻿using AutoMapper;
using FIHS.Dtos.DiseaseDto;
using FIHS.Interfaces;
using FIHS.Interfaces.IDisease;
using FIHS.Models.Disease;
using Microsoft.EntityFrameworkCore;

namespace FIHS.Services.DiseaseService
{
    public class DiseaseService : IDiseaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;
        private readonly string _baseUrl;

        public DiseaseService(ApplicationDbContext context, IImageService imageService, IMapper mapper,IConfiguration configuration)
        {
            _context = context;
            _imageService = imageService;
            _mapper = mapper;
            _baseUrl = configuration["BaseUrl"];
        }

        public async Task<ReturnDiseaseDto> AddDiseaseAsync(DiseaseDto diseaseDto)
        {
            if (await _context.Diseases.AnyAsync(d => d.Name.ToLower().Trim() == diseaseDto.Name.ToLower().Trim()))
                return new ReturnDiseaseDto { Message = "This disease already exists" };
            var disease=_mapper.Map<Disease>(diseaseDto);
            disease.ImageUrl=_imageService.SetImage(diseaseDto.Image);
            await _context.AddAsync(disease);
            await _context.SaveChangesAsync();
            var diseaseView=_mapper.Map<ReturnDiseaseDto>(disease);
            diseaseView.ImageUrl = _baseUrl + disease.ImageUrl;
            return diseaseView;
        }

        public async Task<ReturnDiseaseDto> DeleteDiseaseAsync(int id)
        {
            var disease=await _context.Diseases.SingleOrDefaultAsync(d=>d.Id==id);
            if (disease == null)
                return new ReturnDiseaseDto { Message = "No Disease is Found" };
            _imageService.DeleteImage(disease.ImageUrl);
            _context.Remove(disease);
            await _context.SaveChangesAsync();
            var diseaseView = _mapper.Map<ReturnDiseaseDto>(disease);
            diseaseView.ImageUrl = _baseUrl + disease.ImageUrl;
            return diseaseView;
        }
        public async Task<ReturnDiseaseDto> GetDiseaseByIdAsync(int id)
        {
            var disease = await _context.Diseases.SingleOrDefaultAsync(d => d.Id == id);
            if (disease == null)
                return new ReturnDiseaseDto { Message = "No disease is found" };
            var diseaseView = _mapper.Map<ReturnDiseaseDto>(disease);
            diseaseView.ImageUrl = _baseUrl + disease.ImageUrl;
            return diseaseView;
        }

        public async Task<ReturnDiseaseDto> GetDiseaseByNameAsync(string name)
        {
            var disease =await _context.Diseases.SingleOrDefaultAsync(d => d.Name.ToLower().Trim() == name.ToLower().Trim());
            if (disease == null)
                return new ReturnDiseaseDto { Message = "No Disease is Found" };
            var diseaseView = _mapper.Map<ReturnDiseaseDto>(disease);
            diseaseView.ImageUrl = _baseUrl + disease.ImageUrl;
            return diseaseView;
        }

        public async Task<IEnumerable<ReturnDiseaseDto>> GetDiseasesAsync()
        {
            var diseases = await _context.Diseases.Select(d=>new ReturnDiseaseDto
            {
                Id=d.Id,
                Description=d.Description,
                Name=d.Name,
                PreventionMethods=d.PreventionMethods,
                Symptoms=d.Symptoms,
                Treatments=d.Treatments,
                ImageUrl=_baseUrl + d.ImageUrl,
            }).ToListAsync();
            return diseases;
        }

        public async Task<IEnumerable<ReturnDiseaseDto>> SearchForDiseaseByNameAsync(string name)
        {
            var diseases = await _context.Diseases.Where(d=>d.Name.ToLower().Trim().Contains(name.ToLower().Trim())).Select(d => new ReturnDiseaseDto
            {
                Id = d.Id,
                Description = d.Description,
                Name = d.Name,
                PreventionMethods = d.PreventionMethods,
                Symptoms = d.Symptoms,
                Treatments = d.Treatments,
                ImageUrl = _baseUrl + d.ImageUrl,
            }).ToListAsync();
            return diseases;
        }

        public async Task<ReturnDiseaseDto> UpdateDiseaseAsync(UpdateDiseaseDto diseaseDto, int id)
        {
            var disease = await _context.Diseases.SingleOrDefaultAsync(d => d.Id == id);
            if (disease == null)
                return new ReturnDiseaseDto { Message = "No Disease is Found" };
            disease.Name = diseaseDto.Name ?? disease.Name;
            disease.PreventionMethods = diseaseDto.PreventionMethods ?? disease.PreventionMethods;
            disease.Description = diseaseDto.Description ?? disease.Description;
            disease.Symptoms = diseaseDto.Symptoms ?? disease.Symptoms;
            disease.Treatments= diseaseDto.Treatments ??disease.Treatments;
            disease.ImageUrl=diseaseDto.Image!=null?_imageService.SetImage(diseaseDto.Image,disease.ImageUrl):disease.ImageUrl;
            _context.Update(disease);
            await _context.SaveChangesAsync();
            var diseaseView = _mapper.Map<ReturnDiseaseDto>(disease);
            diseaseView.ImageUrl = _baseUrl + disease.ImageUrl;
            return diseaseView;
        }
    }
}
