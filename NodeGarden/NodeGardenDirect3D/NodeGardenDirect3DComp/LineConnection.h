#pragma once

#include "Sprite.h"
#include "Node.h"

using namespace Microsoft::WRL;
using namespace DirectX;

class LineConnection : public Sprite
{
public:
    LineConnection(void);
    ~LineConnection(void) {};

    virtual void FormConnection(XMVECTOR node1Pos, XMVECTOR node2Pos, float distance);
    virtual void BreakConnection();

    static float Distance(const XMVECTOR& vector1, const XMVECTOR& vector2);

protected:
    static const float StrokeWeightMin; // the Connectedness value determins the stroke width of the node outline. This number is between strokeWeightMin and strokeWeightMax
    static const float StrokeWeightMax; //

    float m_strokeThickness;
    XMFLOAT2 m_start;
    XMFLOAT2 m_end;
};

