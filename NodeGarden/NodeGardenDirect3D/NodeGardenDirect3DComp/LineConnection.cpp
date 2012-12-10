#include "pch.h"
#include "LineConnection.h"

const float LineConnection::StrokeWeightMin = 2.0f; 
const float LineConnection::StrokeWeightMax = 7.0f;

LineConnection::LineConnection()
{
    m_zDepth = 1.0f;
}

void LineConnection::FormConnection(XMVECTOR node1Pos, XMVECTOR node2Pos, float distance)
{
    m_visible = true;
    // draw a line between 2 nodes. The thickness/alpha varies depending on distance
    m_strokeThickness = Node::Map(distance, 0, MinDist, StrokeWeightMax, StrokeWeightMin);
    
    XMVECTORF32 color = {1, 1, 1, Node::Map(distance, 0, MinDist, 1.0f, 0)};
    m_color = color;

    XMStoreFloat2(&m_start, node1Pos);
    XMStoreFloat2(&m_end, node2Pos);

    m_destRect.left =   (long)m_start.x;
    m_destRect.top =    (long)m_start.y;
    m_destRect.right  = (long)(m_start.x + m_strokeThickness);  
    m_destRect.bottom = (long)(m_start.y + Distance(XMLoadFloat2(&m_start), XMLoadFloat2(&m_end)));

    m_rotation = PIOVER2 - (float)atan2(m_end.y - m_start.y, m_start.x - m_end.x);
}

void LineConnection::BreakConnection()
{
    m_visible = false;
}

float LineConnection::Distance(const XMVECTOR& vector1,const XMVECTOR& vector2)
{
    XMVECTOR vectorSub = XMVectorSubtract(vector1,vector2);
    XMVECTOR length = XMVector3Length(vectorSub);

    float distance = 0.0f;
    XMStoreFloat(&distance,length);
    return distance;
}
