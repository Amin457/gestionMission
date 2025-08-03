using AutoMapper;
using FluentValidation;
using gestionMissionBack.Application.DTOs.Article;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Validators;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Domain.Helpers;
using gestionMissionBack.Domain.DTOs;
using gestionMissionBack.Infrastructure.Interfaces;
using gestionMissionBack.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IValidator<ArticleCreateDto> _articleCreateValidator;
        private readonly IValidator<ArticleUpdateDto> _articleUpdateValidator;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public ArticleService(
            IArticleRepository articleRepository,
            IValidator<ArticleCreateDto> articleCreateValidator,
            IValidator<ArticleUpdateDto> articleUpdateValidator,
            IMapper mapper,
            IWebHostEnvironment env)
        {
            _articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
            _articleCreateValidator = articleCreateValidator ?? throw new ArgumentNullException(nameof(articleCreateValidator));
            _articleUpdateValidator = articleUpdateValidator ?? throw new ArgumentNullException(nameof(articleUpdateValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task<ArticleGetDto> GetArticleByIdAsync(int id)
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null)
                throw new KeyNotFoundException($"Article with ID {id} not found");

            return new ArticleGetDto
            {
                ArticleId = article.ArticleId,
                Name = article.Name,
                Description = article.Description,
                Quantity = article.Quantity,
                Weight = article.Weight,
                Volume = article.Volume,
                PhotoUrls = !string.IsNullOrEmpty(article.PhotoUrls) 
                    ? JsonSerializer.Deserialize<List<string>>(article.PhotoUrls) ?? new List<string>()
                    : new List<string>()
            };
        }

        public async Task<IEnumerable<ArticleGetDto>> GetAllArticlesAsync()
        {
            var articles = await _articleRepository.GetAllAsync();
            return articles.Select(article => new ArticleGetDto
            {
                ArticleId = article.ArticleId,
                Name = article.Name,
                Description = article.Description,
                Quantity = article.Quantity,
                Weight = article.Weight,
                Volume = article.Volume,
                PhotoUrls = !string.IsNullOrEmpty(article.PhotoUrls) 
                    ? JsonSerializer.Deserialize<List<string>>(article.PhotoUrls) ?? new List<string>()
                    : new List<string>()
            });
        }

        public async Task<ArticleGetDto> CreateArticleAsync(ArticleCreateDto articleCreateDto)
        {
            var validationResult = await _articleCreateValidator.ValidateAsync(articleCreateDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var photoUrls = new List<string>();
            if (articleCreateDto.Photos != null && articleCreateDto.Photos.Any())
            {
                foreach (var photo in articleCreateDto.Photos)
                {
                    if (photo != null && photo.Length > 0)
                    {
                        var fileName = await StoreFileAsync(photo);
                        photoUrls.Add($"uploads/{fileName}");
                    }
                }
            }

            var article = new Article
            {
                Name = articleCreateDto.Name,
                Description = articleCreateDto.Description,
                Quantity = articleCreateDto.Quantity,
                Weight = articleCreateDto.Weight,
                Volume = articleCreateDto.Volume,
                PhotoUrls = JsonSerializer.Serialize(photoUrls)
            };

            var createdArticle = await _articleRepository.AddAsync(article);
            return await GetArticleByIdAsync(createdArticle.ArticleId);
        }

        public async Task UpdateArticleAsync(ArticleUpdateDto articleUpdateDto)
        {
            var validationResult = await _articleUpdateValidator.ValidateAsync(articleUpdateDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingArticle = await _articleRepository.GetByIdAsync(articleUpdateDto.ArticleId);
            if (existingArticle == null)
                throw new KeyNotFoundException($"Article with ID {articleUpdateDto.ArticleId} not found");

            // Keep existing photos
            var photoUrls = articleUpdateDto.KeepPhotosUrls ?? new List<string>();

            // Add new photos
            if (articleUpdateDto.NewPhotos != null && articleUpdateDto.NewPhotos.Any())
            {
                foreach (var photo in articleUpdateDto.NewPhotos)
                {
                    if (photo != null && photo.Length > 0)
                    {
                        var fileName = await StoreFileAsync(photo);
                        photoUrls.Add($"uploads/{fileName}");
                    }
                }
            }

            existingArticle.Name = articleUpdateDto.Name;
            existingArticle.Description = articleUpdateDto.Description;
            existingArticle.Quantity = articleUpdateDto.Quantity;
            existingArticle.Weight = articleUpdateDto.Weight;
            existingArticle.Volume = articleUpdateDto.Volume;
            existingArticle.PhotoUrls = JsonSerializer.Serialize(photoUrls);

            var updated = await _articleRepository.UpdateAsync(existingArticle);
            if (!updated)
            {
                throw new Exception($"Failed to update article with ID {articleUpdateDto.ArticleId}");
            }
        }

        public async Task DeleteArticleAsync(int id)
        {
            var deleted = await _articleRepository.DeleteAsync(id);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Article with ID {id} not found or could not be deleted");
            }
        }

        public async Task<PagedResult<ArticleGetDto>> GetPagedAsync(int pageNumber, int pageSize, ArticleFilter filter = null)
        {
            var articles = await _articleRepository.GetPagedAsync(pageNumber, pageSize, filter);

            return new PagedResult<ArticleGetDto>
            {
                Data = articles.Data.Select(article => new ArticleGetDto
                {
                    ArticleId = article.ArticleId,
                    Name = article.Name,
                    Description = article.Description,
                    Quantity = article.Quantity,
                    Weight = article.Weight,
                    Volume = article.Volume,
                    PhotoUrls = !string.IsNullOrEmpty(article.PhotoUrls) 
                        ? JsonSerializer.Deserialize<List<string>>(article.PhotoUrls) ?? new List<string>()
                        : new List<string>()
                }),
                TotalRecords = articles.TotalRecords
            };
        }

        private async Task<string> StoreFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}