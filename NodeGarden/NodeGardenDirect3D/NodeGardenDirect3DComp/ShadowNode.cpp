#include "pch.h"
#include "ShadowNode.h"

ShadowNode::ShadowNode(XMFLOAT2 screenSize) : Node(screenSize)
{
    m_zDepth = 0.1f;
}

ShadowNode::ShadowNode(XMFLOAT2 screenSize, XMFLOAT2 position) : Node(screenSize)
{
    m_zDepth = 0.1f;
	m_position = position;
}

void ShadowNode::FinishConnection()
{
    // calculate the shadow sizes from NormalisedConnectedness
    m_outlineSize = m_size + Map(m_normalisedConnectedness, 0, 1, (float)EllipseOutlineMin, (float)EllipseOutlineMax);
    m_shadow1Size = (m_normalisedConnectedness * Shadow1Multiplier);
    m_shadow2Size = (m_normalisedConnectedness * Shadow2Multiplier);
   
    Node::FinishConnection();
}

void ShadowNode::DrawSprites(SpriteBatch* sb, ID3D11ShaderResourceView* texture)
{
    // center all the ellipses on our position taking in to account their sizes
    m_halfSize = m_size / 2;

    m_destRect.left =   (long)(m_position.x - m_halfSize);  
    m_destRect.top =    (long)(m_position.y - m_halfSize);  
    m_destRect.right  = (long)(m_destRect.left + m_size);  
    m_destRect.bottom = (long)(m_destRect.top + m_size);
    sb->Draw(texture, m_destRect, NULL, m_color, 0.0f, XMFLOAT2(0,0), SpriteEffects_None, m_zDepth);
    
    m_halfSize -= (m_size - m_outlineSize) / 2;

    m_destRect.left =   (long)(m_position.x - m_halfSize);  
    m_destRect.top =    (long)(m_position.y - m_halfSize);  
    m_destRect.right  = (long)(m_destRect.left + m_outlineSize);  
    m_destRect.bottom = (long)(m_destRect.top + m_outlineSize);
    XMVECTORF32 color = {0.6f, 0.6f, 0.6f, 1.0f};
    sb->Draw(texture, m_destRect, NULL, color, 0.0f, XMFLOAT2(0,0), SpriteEffects_None, 0.2f);
            
    m_halfSize -= (m_outlineSize - m_shadow1Size) / 2;

    m_destRect.left =   (long)(m_position.x - m_halfSize);  
    m_destRect.top =    (long)(m_position.y - m_halfSize);  
    m_destRect.right  = (long)(m_destRect.left + m_shadow1Size);  
    m_destRect.bottom = (long)(m_destRect.top + m_shadow1Size);
    color.f[0] = 1.0f; color.f[1] = 1.0f; color.f[2] = 1.0f; color.f[3] = 0.3f;
    sb->Draw(texture, m_destRect, NULL, color, 0.0f, XMFLOAT2(0,0), SpriteEffects_None, 0.3f);
            
    m_halfSize -= (m_shadow1Size - m_shadow2Size) / 2;

    m_destRect.left =   (long)(m_position.x - m_halfSize);  
    m_destRect.top =    (long)(m_position.y - m_halfSize);  
    m_destRect.right  = (long)(m_destRect.left + m_shadow2Size);  
    m_destRect.bottom = (long)(m_destRect.top + m_shadow2Size);
    color.f[3] = 0.2f;
    sb->Draw(texture, m_destRect, NULL, color, 0.0f, XMFLOAT2(0,0), SpriteEffects_None, 0.4f);
    
    MaxConnectedness -= 0.00001f;
}