#include "pch.h"
#include "Sprite.h"


Sprite::Sprite(void)
{
    RECT m_destRect = {0,0,0,0};
    m_color = Colors::White;
    m_zDepth = 0.0f;
}

void Sprite::DrawSprites(SpriteBatch* sb, ID3D11ShaderResourceView* texture)
{
    if(m_visible)
    {
        sb->Draw(texture, m_destRect, NULL, m_color, m_rotation, XMFLOAT2(0,0), SpriteEffects_None, m_zDepth);
    }
}