#pragma once
#include "shadownode.h"
#include "LineConnection.h"

class MyNode : public ShadowNode
{
public:
    MyNode(void) {};
    explicit MyNode(XMFLOAT2 screenSize);
    ~MyNode(void){};

    virtual void OnPointerPressed(XMFLOAT2) override;
    virtual void OnPointerMoved(XMFLOAT2) override;
    virtual void OnPointerReleased() override;

    virtual void Update(float timeTotal, float timeDelta) override {};

private:
    bool m_isBeingDragged;
    static const int TouchAreaSize = 60;
};

