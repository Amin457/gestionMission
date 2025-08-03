using gestionMissionBack.Application.DTOs.Article;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Services;
using gestionMissionBack.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gestionMissionBack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticlesController(IArticleService articleService)
        {
            _articleService = articleService ?? throw new ArgumentNullException(nameof(articleService));
        }

        // GET: api/articles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArticleGetDto>>> GetAllArticles()
        {
            var articles = await _articleService.GetAllArticlesAsync();
            return Ok(articles);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] ArticleFilter filter = null)
        {
            var pagedArticles = await _articleService.GetPagedAsync(pageNumber, pageSize, filter);
            return Ok(pagedArticles);
        }

        // GET: api/articles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleGetDto>> GetArticle(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            return Ok(article);
        }

        // POST: api/articles (for articles with photos - multipart/form-data)
        [HttpPost]
        public async Task<ActionResult<ArticleGetDto>> CreateArticle([FromForm] ArticleCreateDto articleCreateDto)
        {
            var createdArticle = await _articleService.CreateArticleAsync(articleCreateDto);
            return CreatedAtAction(nameof(GetArticle), new { id = createdArticle.ArticleId }, createdArticle);
        }

        // POST: api/articles/json (for articles without photos - JSON)
        [HttpPost("json")]
        public async Task<ActionResult<ArticleGetDto>> CreateArticleJson([FromBody] ArticleCreateDto articleCreateDto)
        {
            // Set Photos to null for JSON requests
            articleCreateDto.Photos = null;
            var createdArticle = await _articleService.CreateArticleAsync(articleCreateDto);
            return CreatedAtAction(nameof(GetArticle), new { id = createdArticle.ArticleId }, createdArticle);
        }

        // PUT: api/articles/5 (for articles with photos - multipart/form-data)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArticle(int id, [FromForm] ArticleUpdateDto articleUpdateDto)
        {
            if (id != articleUpdateDto.ArticleId)
            {
                throw new ArgumentException("Route ID in URL must match Article ID in body");
            }

            await _articleService.UpdateArticleAsync(articleUpdateDto);
            return NoContent();
        }

        // PUT: api/articles/5/json (for articles without photos - JSON)
        [HttpPut("{id}/json")]
        public async Task<IActionResult> UpdateArticleJson(int id, [FromBody] ArticleUpdateDto articleUpdateDto)
        {
            if (id != articleUpdateDto.ArticleId)
            {
                throw new ArgumentException("Route ID in URL must match Article ID in body");
            }

            // Set NewPhotos to null for JSON requests
            articleUpdateDto.NewPhotos = null;
            await _articleService.UpdateArticleAsync(articleUpdateDto);
            return NoContent();
        }

        // DELETE: api/articles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            await _articleService.DeleteArticleAsync(id);
            return NoContent();
        }
    }
}