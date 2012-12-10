#pragma once

#include "Direct3DBase.h"
#include "SpriteBatch.h"
#include "Effects.h"
#include "PrimitiveBatch.h"
#include "VertexTypes.h"
#include "ShadowNode.h"
#include "MyNode.h"
#include "LineConnection.h"
#include "DDSTextureLoader.h"
#include <time.h>

//#define NodeNum 50
//#define LineNum (((NodeNum)*((NodeNum)-1))/2)

using namespace DirectX;

// This class renders sprites and primitives using the DirectXTK
ref class XTKRenderer sealed : public Direct3DBase
{
public:
    XTKRenderer(void);
    virtual ~XTKRenderer(void);

    // Event handlers
    void OnPointerPressed(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ sender, Windows::UI::Core::PointerEventArgs^ args);
	void OnPointerMoved(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ sender, Windows::UI::Core::PointerEventArgs^ args);
	void OnPointerReleased(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ sender, Windows::UI::Core::PointerEventArgs^ args);

    // Direct3DBase methods.
    virtual void CreateDeviceResources(void) override;
    virtual void CreateWindowSizeDependentResources(void) override;
    virtual void Render(void) override;
	int CreateNode(float nodeX, float nodeY);
	void RemoveNode(int nativeId);

    // Method for updating time-dependent objects.
    void Update(float timeTotal, float timeDelta);

    void ChangeNodeAmount(int newAmount);
    Windows::Foundation::Point GetMyNodePosition();
    Windows::Foundation::Point CreateMyNode();
    void UpdateNodePosition(int nodeId, float nodeX, float nodeY);

    bool IsLoaded();

private:
    Microsoft::WRL::ComPtr<ID3D11ShaderResourceView> m_pTextureView;
    std::unique_ptr<SpriteBatch> m_pSpriteBatch;
    Microsoft::WRL::ComPtr<ID3D11BlendState> m_pBlendState;
    Microsoft::WRL::ComPtr<ID3D11ShaderResourceView> m_pTexture;

    XMMATRIX m_world;
    XMMATRIX m_view; 
    XMMATRIX m_projection;

    int NodeNum;
    int LineNum;

    std::vector<ShadowNode*> m_nodes;
    std::vector<LineConnection*> m_lines;

    bool m_isLoaded;
};