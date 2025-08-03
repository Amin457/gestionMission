using AutoMapper;
using FluentValidation;
using gestionMissionBack.Application.DTOs.Document;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Validators;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly DocumentCreateValidator _documentCreateValidator;
        private readonly DocumentUpdateValidator _documentUpdateValidator;
        private readonly IWebHostEnvironment _env;

        public DocumentService(
            IDocumentRepository documentRepository,
            DocumentCreateValidator documentCreateValidator,
            DocumentUpdateValidator documentUpdateValidator,
            IWebHostEnvironment env)
        {
            _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
            _documentCreateValidator = documentCreateValidator ?? throw new ArgumentNullException(nameof(documentCreateValidator));
            _documentUpdateValidator = documentUpdateValidator ?? throw new ArgumentNullException(nameof(documentUpdateValidator));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task<DocumentGetDto> GetDocumentByIdAsync(int id)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            return new DocumentGetDto
            {
                DocumentId = document.DocumentId,
                TaskId = document.TaskId,
                Name = document.Name,
                Type = document.Type,
                StoragePath = document.StoragePath,
                AddedDate = document.AddedDate
            };
        }

        public async Task<IEnumerable<DocumentGetDto>> GetAllDocumentsAsync()
        {
            var documents = await _documentRepository.GetAllAsync();
            return documents.Select(document => new DocumentGetDto
            {
                DocumentId = document.DocumentId,
                TaskId = document.TaskId,
                Name = document.Name,
                Type = document.Type,
                StoragePath = document.StoragePath,
                AddedDate = document.AddedDate
            });
        }

        public async Task<IEnumerable<DocumentGetDto>> GetDocumentsByTaskIdAsync(int taskId)
        {
            var documents = await _documentRepository.GetDocumentsByTaskIdAsync(taskId);
            return documents.Select(document => new DocumentGetDto
            {
                DocumentId = document.DocumentId,
                TaskId = document.TaskId,
                Name = document.Name,
                Type = document.Type,
                StoragePath = document.StoragePath,
                AddedDate = document.AddedDate
            });
        }

        public async Task<int> GetDocumentCountByTaskIdAsync(int taskId)
        {
            return await _documentRepository.GetDocumentCountByTaskIdAsync(taskId);
        }

        public async Task<string> StoreFileAsync(IFormFile file)
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

        public async Task<DocumentDto> CreateDocumentWithFileAsync(DocumentCreateDto createDto)
        {
            var validationResult = await _documentCreateValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            if (createDto.File == null || createDto.File.Length == 0)
                throw new ArgumentException("File is required");

            // Store file and get file name
            var fileName = await StoreFileAsync(createDto.File);
            var storagePath = $"uploads/{fileName}";

            var document = new Document
            {
                TaskId = createDto.TaskId,
                Name = createDto.Name,
                Type = createDto.Type,
                StoragePath = storagePath,
                AddedDate = DateTime.UtcNow
            };

            var createdDocument = await _documentRepository.AddAsync(document);
            return new DocumentDto
            {
                DocumentId = createdDocument.DocumentId,
                TaskId = createdDocument.TaskId,
                Name = createdDocument.Name,
                Type = createdDocument.Type,
                StoragePath = createdDocument.StoragePath,
                AddedDate = createdDocument.AddedDate
            };
        }

        public async Task DeleteDocumentAsync(int id)
        {
            var deleted = await _documentRepository.DeleteAsync(id);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Document with ID {id} not found or could not be deleted");
            }
        }

        public async Task<DocumentDto> CreateDocumentAsync(DocumentCreateDto createDto)
        {
            var validationResult = await _documentCreateValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var fileName = await StoreFileAsync(createDto.File);
            var storagePath = $"uploads/{fileName}";

            var document = new Document
            {
                TaskId = createDto.TaskId,
                Name = createDto.Name,
                Type = createDto.Type,
                StoragePath = storagePath,
                AddedDate = DateTime.UtcNow
            };

            var createdDocument = await _documentRepository.AddAsync(document);
            return new DocumentDto
            {
                DocumentId = createdDocument.DocumentId,
                TaskId = createdDocument.TaskId,
                Name = createdDocument.Name,
                Type = createdDocument.Type,
                StoragePath = createdDocument.StoragePath,
                AddedDate = createdDocument.AddedDate
            };
        }

        public async Task UpdateDocumentAsync(DocumentDto documentDto)
        {
            var validationResult = await _documentUpdateValidator.ValidateAsync(documentDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingDocument = await _documentRepository.GetByIdAsync(documentDto.DocumentId);
            if (existingDocument == null)
                throw new KeyNotFoundException($"Document with ID {documentDto.DocumentId} not found");

            existingDocument.TaskId = documentDto.TaskId;
            existingDocument.Name = documentDto.Name;
            existingDocument.Type = documentDto.Type;
            existingDocument.AddedDate = DateTime.UtcNow;

            if (documentDto.NewFile != null)
            {
                var fileName = await StoreFileAsync(documentDto.NewFile);
                existingDocument.StoragePath = $"uploads/{fileName}";
            }
            else if (!string.IsNullOrEmpty(documentDto.KeptFile))
            {
                existingDocument.StoragePath = documentDto.KeptFile;
            }

            var updated = await _documentRepository.UpdateAsync(existingDocument);
            if (!updated)
            {
                throw new Exception($"Failed to update document with ID {documentDto.DocumentId}");
            }
        }
    }
}