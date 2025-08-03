using gestionMissionBack.Application.DTOs.Document;
using gestionMissionBack.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        }

        // GET: api/documents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentGetDto>>> GetAllDocuments()
        {
            var documents = await _documentService.GetAllDocumentsAsync();
            return Ok(documents);
        }

        // GET: api/documents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentGetDto>> GetDocument(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            return Ok(document);
        }

        // GET: api/documents/task/5
        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<IEnumerable<DocumentGetDto>>> GetDocumentsByTaskId(int taskId)
        {
            var documents = await _documentService.GetDocumentsByTaskIdAsync(taskId);
            return Ok(documents);
        }

        // GET: api/documents/count/task/5
        [HttpGet("count/task/{taskId}")]
        public async Task<ActionResult<int>> GetDocumentCountByTaskId(int taskId)
        {
            var count = await _documentService.GetDocumentCountByTaskIdAsync(taskId);
            return Ok(count);
        }

        // POST: api/documents
        [HttpPost]
        public async Task<ActionResult<DocumentDto>> CreateDocument([FromForm] DocumentCreateDto document)
        {
            var createdDocument = await _documentService.CreateDocumentAsync(document);
            return CreatedAtAction(nameof(GetDocument), new { id = createdDocument.DocumentId }, createdDocument);
        }

        // PUT: api/documents/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, [FromBody] DocumentDto documentDto)
        {
            if (id != documentDto.DocumentId)
            {
                throw new ArgumentException("Route ID in URL must match Route ID in body");
            }

            await _documentService.UpdateDocumentAsync(documentDto);
            return NoContent();
        }

        // DELETE: api/documents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            await _documentService.DeleteDocumentAsync(id);
            return NoContent();
        }
    }
}