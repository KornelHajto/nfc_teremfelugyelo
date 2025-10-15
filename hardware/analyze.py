import cv2
import numpy as np
from insightface.app import FaceAnalysis
from typing import Union, Tuple, Dict

# Lazy-initialized InsightFace app (so importing this module is cheap)
_APP = None

def _get_app(model: str = "buffalo_s") -> FaceAnalysis:
    global _APP
    if _APP is None:
        _APP = FaceAnalysis(name=model, providers=['CPUExecutionProvider'])
        _APP.prepare(ctx_id=0, det_size=(320, 320))
    return _APP


def _load_image(img: Union[str, np.ndarray]):
    """Load an image from path or pass-through if already an ndarray."""
    if isinstance(img, str):
        im = cv2.imread(img)
        if im is None:
            raise FileNotFoundError(f"Could not read image from path: {img}")
        return im
    if isinstance(img, np.ndarray):
        return img
    raise TypeError("img must be a file path or a numpy.ndarray")


def compare_faces(img1: Union[str, np.ndarray], img2: Union[str, np.ndarray], *,
                  model: str = "buffalo_s", threshold: float = 0.3,
                  return_embeddings: bool = False) -> Dict:
    """Compare two images and return similarity info.

    Args:
      img1, img2: path (str) or numpy.ndarray of the images.
      model: insightface model name to use.
      threshold: cosine similarity threshold to consider a match.
      return_embeddings: if True include embeddings as lists in the result.

    Returns a dict with keys:
      status: 'ok' or 'error'
      similarity: cosine similarity (float)
      match: bool (similarity > threshold)
      message: optional error message
      embeddings: optional (list, list) if return_embeddings True
    """
    app = _get_app(model)
    try:
        im1 = _load_image(img1)
        im2 = _load_image(img2)
    except Exception as e:
        return {'status': 'error', 'message': str(e)}

    faces1 = app.get(im1)
    faces2 = app.get(im2)

    if not faces1 or not faces2:
        return {'status': 'error', 'message': 'no_face_detected_in_one_or_both_images'}

    emb1 = faces1[0].embedding
    emb2 = faces2[0].embedding

    # compute cosine similarity
    sim = float(np.dot(emb1, emb2) / (np.linalg.norm(emb1) * np.linalg.norm(emb2)))
    match = sim > threshold

    out = {'status': 'ok', 'similarity': sim, 'match': bool(match)}
    if return_embeddings:
        out['embeddings'] = (emb1.tolist(), emb2.tolist())
    return out


if __name__ == '__main__':
    import sys
    if len(sys.argv) < 3:
        print('Usage: python analyze.py <image1> <image2> [threshold]')
        sys.exit(1)
    img1_path = sys.argv[1]
    img2_path = sys.argv[2]
    thr = float(sys.argv[3]) if len(sys.argv) > 3 else 0.3
    res = compare_faces(img1_path, img2_path, threshold=thr)
    if res.get('status') != 'ok':
        print('❌', res.get('message'))
        sys.exit(2)
    sim = res['similarity']
    print(f"Cosine similarity: {sim:.3f}")
    if res['match']:
        print('✅ Likely the same person.')
    else:
        print('❌ Probably different people.')
