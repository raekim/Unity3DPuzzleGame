using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public GameObject debris;
    public AnimationCurve protectCurve;
    public Shader whiteShader;

    public bool isProtected = false;
    public bool isCracked = false;  // 잘못 부숴서 금이 간 조각
    public bool isAnswerCube = false;   // 정답에 속하는 조각인가?
    public bool isDestroyed = false;

    public Material crackMat;

    public MeshRenderer[] faceMeshRanders;

    enum FACE_INDEX
    {
        TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK
    }

    public enum MATERIAL_INDEX
    {
        BACKGROUND, NUMBERCLUE, CIRCLE_SQAURE, CRACK, MAX
    }

    bool shaking = false; 

    float rand = .06f;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            SetShader(whiteShader);
        }
    }

    public void GetCracked()
    {
        if (isCracked) return;

        isCracked = true;

        Material crackMaterial = Resources.Load<Material>("cubeFace/Materials/crack");

        // 큐브에 금이 간 텍스쳐 적용
        foreach (MeshRenderer renderer in faceMeshRanders)
        {
            Material[] materialsArray = renderer.materials;

            materialsArray[(int)MATERIAL_INDEX.CRACK] = crackMat;
            materialsArray[(int)MATERIAL_INDEX.CRACK].shader = Shader.Find("Sprites/Default");

            renderer.materials = materialsArray;
        }
    }

    //public void EraseAllClues()
    //{
    //    var blankClueTexture = (Material)Resources.Load("cubeFace/gray");
    //    foreach(var renderer in faceMeshRanders)
    //    {
    //        renderer.materials[]
    //    }
    //    right.materials[0] = blankClueTexture;
    //    left.materials[0] = blankClueTexture;
    //    top.materials[0] = blankClueTexture;
    //    bottom.materials[0] = blankClueTexture;
    //    front.materials[0] = blankClueTexture;
    //    back.materials[0] = blankClueTexture;
    //}

    private IEnumerator shakeAnimation()
    {
        shaking = true;

        var beforePos = transform.localPosition;

        float t = 0f;

        while (t < .1f)
        {
            t += Time.deltaTime;
            transform.localPosition = beforePos + new Vector3(Random.Range(-rand, rand), Random.Range(-rand, rand), Random.Range(-rand, rand));
            yield return null;
        }

        while (t > 0f)
        {
            t -= Time.deltaTime;
            transform.localPosition = beforePos + new Vector3(Random.Range(-rand, rand), Random.Range(-rand, rand), Random.Range(-rand, rand));
            yield return null;
        }

        transform.localPosition = beforePos;

        shaking = false;
    }

    private IEnumerator protectAnimation()
    {
        float t = 0f;

        while (t < .1f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.one *(1 + protectCurve.Evaluate(t)/.1f/6f);
            yield return null;
        }

        while (t > 0f)
        {
            t -= Time.deltaTime;
            transform.localScale = Vector3.one * (1 + protectCurve.Evaluate(t)/.1f/6f);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    private IEnumerator DestroyEffect()
    {
        SetShader(whiteShader);
        yield return new WaitForSeconds(.1f);

        Destroy(Instantiate(debris, transform.position, Quaternion.identity), 3f);

        // 하얀 큐브에서 원래대로 돌아오기
        foreach(MeshRenderer renderer in faceMeshRanders)
        {
            foreach(Material mat in renderer.materials)
            {
                mat.shader = Shader.Find("Standard");
            }

            renderer.materials[(int)MATERIAL_INDEX.BACKGROUND].shader = Shader.Find("Sprites/Default");
        }
        gameObject.SetActive(false);
    }

    public void PlayProtectAnimation()
    {
        StartCoroutine(protectAnimation());
    }

    public void PlayshakeAnimation()
    {
        if (!shaking) StartCoroutine(shakeAnimation());
    }

    public void SetShader(Shader s)
    {
        foreach(MeshRenderer renderer in faceMeshRanders)
        {
            foreach(Material m in renderer.materials)
            {
                m.shader = s;
            }
        }
    }

    public void PlayDestoryEffect()
    {
        StartCoroutine(DestroyEffect());
    }

    public void SetColor(Color _color, int materialIndex)
    {
        foreach(MeshRenderer renderer in faceMeshRanders)
        {
            var materialArray = renderer.materials;
            materialArray[materialIndex].color = _color;
            renderer.materials = materialArray;
        }
    }

    // 퍼즐 위에 정답 색상을 덧칠하기 전 사전 작업
    public void ClearFaces()
    {
        var whiteFaceMaterial = (Material)Resources.Load("cubeFace/whiteFace");

        foreach(MeshRenderer renderer in faceMeshRanders)
        {
            // 엣지가 그려진 회색 큐브 대신 하얀색 배경 material로 대체
            var materialArray = renderer.materials;

            renderer.materials[(int)MATERIAL_INDEX.BACKGROUND] = whiteFaceMaterial;

            for(int i = (int)MATERIAL_INDEX.BACKGROUND + 1 ; i< renderer.materials.Length; ++i)
            {
                // 면의 모든 material을 null로 설정
                renderer.materials[i] = null;
            }

            renderer.materials = materialArray;
        }
    }

    public void HideNumberClueOnCertainFace(PuzzleSolver.FACE_TYPE face)
    {
        switch(face)
        {
            case PuzzleSolver.FACE_TYPE.FRONT_BACK:
                HideNumberClueCall(faceMeshRanders[(int)FACE_INDEX.FRONT]);
                HideNumberClueCall(faceMeshRanders[(int)FACE_INDEX.BACK]);
                break;
            case PuzzleSolver.FACE_TYPE.TOP_BOTTOM:
                HideNumberClueCall(faceMeshRanders[(int)FACE_INDEX.TOP]);
                HideNumberClueCall(faceMeshRanders[(int)FACE_INDEX.BOTTOM]);
                break;
            case PuzzleSolver.FACE_TYPE.RIGHT_LEFT:
                HideNumberClueCall(faceMeshRanders[(int)FACE_INDEX.RIGHT]);
                HideNumberClueCall(faceMeshRanders[(int)FACE_INDEX.LEFT]);
                break;
        }
    }

    void HideNumberClueCall(MeshRenderer renderer)
    {
        // 숫자 단서를 지운다
        Material[] materialsArray = renderer.materials;

        materialsArray[(int)MATERIAL_INDEX.NUMBERCLUE] = null;

        // 원 또는 사각형 모양을 지운다
        if (renderer.materials.Length > 2)
        {
            materialsArray[(int)MATERIAL_INDEX.CIRCLE_SQAURE] = null;
        }

        renderer.materials = materialsArray;
    }

    public void HideNumberClue()
    {
        // 그대로 둘 것 : 엣지가 그려진 회색 큐브 배경 material과, 금이 간 material은
        foreach (MeshRenderer renderer in faceMeshRanders)
        {
            HideNumberClueCall(renderer);
        }
    }

    public IEnumerator WhiteFlash()
    {
        foreach (MeshRenderer renderer in faceMeshRanders)
        {
            // 숫자 단서를 지운다
            Material[] materialsArray = renderer.materials;

            // 금이 간 material을 지운다
            materialsArray[(int)MATERIAL_INDEX.CRACK] = null;

            renderer.materials = materialsArray;
        }

        SetShader(Shader.Find("Sprites/Diffuse"));
        ClearFaces();
        Color originalColor = faceMeshRanders[0].materials[(int)MATERIAL_INDEX.BACKGROUND].color;

        Debug.Log(originalColor);
    
        float t = 0f;
    
        while(t < 1.5f)
        {
            SetColor(Color.Lerp(Color.white, originalColor, t / 1.5f), 0);
    
            t += Time.deltaTime;
    
            yield return null;
        }
    }

    // num : 해당 라인에서 부숴지지 않아야 할 큐브의 갯수
    // groups : 부숴지지 않아야 할 큐브들이 몇 개의 뭉치로 나누어져있는지 갯수 (2개면 동그라미, 3개 이상이면 네모를 추가로 그림)

    void SetNumberClue(int faceIndex, int num, Puzzle.CLUE_SHAPE shape)
    {
        Material[] materialsArray = faceMeshRanders[faceIndex].materials;
        // 숫자 쓰기
        Material numberClueMaterial = Resources.Load<Material>(@"cubeFace/Materials/clue-" + num.ToString());
        materialsArray[(int)MATERIAL_INDEX.NUMBERCLUE] = numberClueMaterial;

        // 모양 그리기
        switch(shape)
        {
            case Puzzle.CLUE_SHAPE.NONE:
                break;
            case Puzzle.CLUE_SHAPE.CIRCLE:
                {
                    Material circleMaterial = Resources.Load<Material>("cubeFace/Materials/circle");
                    materialsArray[(int)MATERIAL_INDEX.CIRCLE_SQAURE] = circleMaterial;
                }
                break;
            case Puzzle.CLUE_SHAPE.SQUARE:
                {
                    Material squareMaterial = Resources.Load<Material>("cubeFace/Materials/square");
                    materialsArray[(int)MATERIAL_INDEX.CIRCLE_SQAURE] = squareMaterial;
                }
                break;
        }

        faceMeshRanders[faceIndex].materials = materialsArray;
    }

    public void SetRightLeftNumberClue(int num, Puzzle.CLUE_SHAPE shape)
    {
        if (num < 0) return;

        SetNumberClue((int)FACE_INDEX.RIGHT, num, shape);
        SetNumberClue((int)FACE_INDEX.LEFT, num, shape);
    }

    public void SetTopBottomNumberClue(int num, Puzzle.CLUE_SHAPE shape)
    {
        if (num < 0) return;

        SetNumberClue((int)FACE_INDEX.TOP, num, shape);
        SetNumberClue((int)FACE_INDEX.BOTTOM, num, shape);
    }

    public void SetFrontBackNumberClue(int num, Puzzle.CLUE_SHAPE shape)
    {
        if (num < 0) return;

        SetNumberClue((int)FACE_INDEX.FRONT, num, shape);
        SetNumberClue((int)FACE_INDEX.BACK, num, shape);
    }
}
