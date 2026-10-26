from fastapi import APIRouter
from models.schemas import AnalyticsReportRequest
from services.report_generator import generate_report

router = APIRouter(prefix="/analytics", tags=["Analytics"])

@router.post("/report")
def generate_analytics_report(req: AnalyticsReportRequest):
    return generate_report(req)

@router.get("/summary/{url_id}")
def get_summary(url_id: int):
    return {
        "url_id": url_id,
        "message": "Connect to your DB for live data",
        "metrics": ["total_clicks", "clicks_today", "clicks_this_week", "device_breakdown", "browser_breakdown"]
    }
