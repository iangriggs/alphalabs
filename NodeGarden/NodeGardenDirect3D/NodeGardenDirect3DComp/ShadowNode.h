#pragma once
#include "Node.h"

using namespace DirectX;

class ShadowNode : public Node
{
public:
    ShadowNode(void) {};
    ShadowNode(XMFLOAT2 screenSize);
	ShadowNode(XMFLOAT2 screenSize, XMFLOAT2 position);
    ~ShadowNode(void) {};

    virtual void OnPointerPressed(XMFLOAT2) {};
    virtual void OnPointerMoved(XMFLOAT2) {};
    virtual void OnPointerReleased() {};

    virtual void FinishConnection(void) override;
    virtual void DrawSprites(SpriteBatch* sb, ID3D11ShaderResourceView* texture) override;

protected:
        // Protected Consts
        static const int Shadow1Multiplier = 85;        // shadow 1 size
        static const int Shadow2Multiplier = 110;       // shadow 2 size
        static const int EllipseOutlineMin = 4;         // minimum thickness for the ellipse outline. Mapped using Connectedness
        static const int EllipseOutlineMax = 12;        // 
        
        // Protected Members
        float m_halfSize;
        float m_outlineSize;
        float m_shadow1Size;
        float m_shadow2Size;
};
