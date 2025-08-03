export interface Article {
    articleId: number;
    name: string;
    description: string;
    quantity: number;
    weight: number;
    volume: number;
    photoUrls?: string[];
}

export interface ArticleFilter {
    name?: string;
    description?: string;
    minQuantity?: number;
    maxQuantity?: number;
    minWeight?: number;
    maxWeight?: number;
    minVolume?: number;
    maxVolume?: number;
}