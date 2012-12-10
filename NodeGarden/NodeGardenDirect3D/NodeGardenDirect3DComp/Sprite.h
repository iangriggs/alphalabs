#pragma once

#include "SpriteBatch.h"

using namespace Microsoft::WRL;
using namespace DirectX;

class Sprite
{
public:
    Sprite(void);
    ~Sprite(void) {};

    virtual void DrawSprites(SpriteBatch* sb, ID3D11ShaderResourceView* texture);

protected:
    RECT m_destRect;
    XMVECTORF32 m_color;
    float m_rotation;
    bool m_visible;
    float m_zDepth;
};

