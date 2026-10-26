from models.schemas import AnalyticsReportRequest

def generate_report(req: AnalyticsReportRequest) -> dict:
    peak_day = max(req.daily_clicks, key=lambda x: x.clicks, default=None)
    avg_daily = round(req.total_clicks / max(len(req.daily_clicks), 1), 2)
    top_device = max(req.device_breakdown, key=lambda x: x.count, default=None)

    insights = []
    if req.clicks_today > avg_daily:
        insights.append(f"Today is performing {round((req.clicks_today / avg_daily - 1) * 100)}% above average.")
    if peak_day:
        insights.append(f"Peak day was {peak_day.date} with {peak_day.clicks} clicks.")
    if top_device:
        insights.append(f"Most traffic comes from {top_device.device} ({top_device.percentage}%).")
    if req.total_clicks == 0:
        insights.append("No clicks yet. Share your link to start tracking!")

    return {
        "url_id": req.url_id,
        "short_code": req.short_code,
        "original_url": req.original_url,
        "summary": {
            "total_clicks": req.total_clicks,
            "clicks_today": req.clicks_today,
            "clicks_this_week": req.clicks_this_week,
            "clicks_this_month": req.clicks_this_month,
            "average_daily_clicks": avg_daily,
            "peak_day": peak_day.date if peak_day else None,
            "peak_day_clicks": peak_day.clicks if peak_day else 0,
        },
        "insights": insights,
        "device_breakdown": req.device_breakdown,
        "top_referers": req.top_referers,
    }
