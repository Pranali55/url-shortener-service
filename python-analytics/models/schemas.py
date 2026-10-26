from pydantic import BaseModel
from typing import List, Optional

class DailyClick(BaseModel):
    date: str
    clicks: int

class DeviceBreakdown(BaseModel):
    device: str
    count: int
    percentage: float

class AnalyticsReportRequest(BaseModel):
    url_id: int
    short_code: str
    original_url: str
    total_clicks: int
    clicks_today: int
    clicks_this_week: int
    clicks_this_month: int
    daily_clicks: List[DailyClick]
    device_breakdown: List[DeviceBreakdown]
    top_referers: Optional[List[dict]] = []
