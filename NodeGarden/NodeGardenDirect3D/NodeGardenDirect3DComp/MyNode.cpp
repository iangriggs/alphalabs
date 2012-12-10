#include "pch.h"
#include "MyNode.h"


MyNode::MyNode(XMFLOAT2 screenSize) : ShadowNode(screenSize)
{
    m_color.f[0] = random(0.5f) + 0.5f;
    m_color.f[1] = random(0.5f) + 0.5f;
    m_color.f[2] = random(0.5f) + 0.5f;
    m_color.f[3] = 1.0f;

    m_isBeingDragged = false;
    m_zDepth = 0.0f;
	SetUniqueId();
}

void MyNode::OnPointerPressed(XMFLOAT2 touchPosition)
{
    if(LineConnection::Distance(GetPosition(), XMLoadFloat2(&touchPosition)) < TouchAreaSize)
    {
        m_isBeingDragged = true;
    }
}

void MyNode::OnPointerMoved(XMFLOAT2 touchPosition)
{
    if(m_isBeingDragged == true)
    {
        m_position = touchPosition;
    }
}

void MyNode::OnPointerReleased()
{
    m_isBeingDragged = false;
}