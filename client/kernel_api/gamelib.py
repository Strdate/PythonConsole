from __future__ import annotations

import functools
import json
from typing import Any, List, Optional, overload
from typing_extensions import Literal

from .. import meta, model, protocol, xml_
from . import objects, runtime


class Game(metaclass=meta.Singleton):

    _channel = None

    def __init__(self) -> None:
        self._async_mode = False

    @property
    def async_mode(self) -> bool:
        return self._async_mode

    @async_mode.setter
    def async_mode(self, value: bool) -> None:
        self._async_mode = value

    def _remote_call(self,
        contract: protocol.Contract, message: Any
    ) -> Any:
        encoder = xml_.XMLSerializer()
        decoder = xml_.XMLDeserializer()
        async_mode = self._async_mode and contract.can_run_async
        runtime.info(str(vars(contract)))
        builtin = isinstance(message, (bool, str, int, float))
        response = input(json.dumps({
            'contract': vars(contract),
            'message': str(encoder.export(message)) if not builtin else message,
            'async': async_mode,
            'builtin': builtin
        }))
        if response:
            ret = decoder.deserialize(xml_.parse(response))
            return ret
        else:
            return None

    def _get_obj(self, type_: str, id_: int = 0, idString: Optional[str] = None):
        ret = self._remote_call(
            protocol.REMOTE_METHODS['get_object_from_id'],
            model.GetObjectMessage(id=id_, type=type_, idString=idString)
        )
        assert isinstance(ret, model.BaseMessage)
        return ret

    def _move_obj(self, id_: int, type_: str, position: model.IPositionable,
        angle: Optional[float] = None
    ):
        return self._remote_call(
            protocol.REMOTE_METHODS['move_object'],
            model.MoveMessage(
                id=id_, type=type_, position=position,
                angle=0 if angle is None else angle,
                is_angle_defined=angle is not None
            )
        )

    def _delete_obj(self, id_: int, type_: str, keep_nodes: bool = False):
        return self._remote_call(
            protocol.REMOTE_METHODS['delete_object'],
            model.DeleteObjectMessage(id=id_, type=type_, keep_nodes=keep_nodes)
        )

    def _set_resource(self, id_: int, type: str, value: int):
        return self._remote_call(
            protocol.REMOTE_METHODS['set_natural_resource'],
            model.SetNaturalResourceMessage(
                cell_id=id_, type=type, value=value
            )
        )

    def _get_adj_segment(self, id_: int):
        msg = self._remote_call(
            protocol.REMOTE_METHODS['get_segment_for_node_id'], id_
        )
        assert isinstance(msg, list)
        ret = []
        for _ in msg:
            assert isinstance(_, model.BaseMessage)
            ret.append(objects.Segment.from_message(_))
        
        return ret

    def get_prop(self, id_: int) -> objects.Prop:
        return objects.Prop.from_message(self._get_obj(id_=id_, type_='prop'))

    def get_tree(self, id_: int) -> objects.Tree:
        return objects.Tree.from_message(self._get_obj(id_=id_, type_='tree'))

    def get_building(self, id_: int) -> objects.Building:
        return objects.Building.from_message(
            self._get_obj(id_=id_, type_='building')
        )

    def get_node(self, id_: int) -> objects.Node:
        return objects.Node.from_message(self._get_obj(id_=id_, type_='node'))

    def get_segment(self, id_: int) -> objects.Segment:
        return objects.Segment.from_message(self._get_obj(id_=id_, type_='segment'))

    def create_prop(self,
        position: model.IPositionable, prefab_name: str, angle: float
    ):
        ret = self._remote_call(
            protocol.REMOTE_METHODS['create_prop'],
            model.CreatePropMessage(
                Position=position.position,
                Type=prefab_name, Angle=angle
            )
        )
        if isinstance(ret, model.BaseMessage):
            return objects.Prop.from_message(ret)
        return None

    def create_tree(self, position: model.IPositionable, prefab_name: str):
        ret = self._remote_call(
            protocol.REMOTE_METHODS['create_tree'],
            model.CreateTreeMessage(
                Position=position.position,
                prefab_name=prefab_name
            )
        )
        if isinstance(ret, model.BaseMessage):
            return objects.Tree.from_message(ret)
        return None

    def create_building(self,
        position: model.IPositionable, type_: str, angle: float = 0
    ) -> Optional[objects.Building]:
        ret = self._remote_call(
            protocol.REMOTE_METHODS['create_building'],
            model.CreateBuildingMessage(
                Position=position.position,
                Type=type_, Angle=angle
            )
        )
        if isinstance(ret, model.BaseMessage):
            return objects.Building.from_message(ret)
        return None
    
    def create_node(self,
        position: model.IPositionable,
        prefab: str | objects.NetPrefab
    ) -> Optional[objects.Node]:
        if not isinstance(prefab, (str, objects.NetPrefab)):
            raise TypeError("Prefab must be string or NetPrefab")
        ret = self._remote_call(
            protocol.REMOTE_METHODS['create_node'],
            model.CreateBuildingMessage(
                Position=position.position,
                Type=prefab if isinstance(prefab, str) else prefab.name
            )
        )
        if isinstance(ret, model.BaseMessage):
            return objects.Node.from_message(ret)
        return None

    @overload
    def _create_segment(self,
        start_node: model.IPositionable, end_node: model.IPositionable,
        type_: Any, *,
        start_dir: Optional[model.Vector] = None,
        end_dir: Optional[model.Vector] = None,
        middle_pos: Optional[model.IPositionable] = None,
        autosplit: Literal[False] = False
    ) -> Optional[objects.Segment]:
        ...

    @overload
    def _create_segment(self,
        start_node: model.IPositionable, end_node: model.IPositionable,
        type_: Any, *,
        start_dir: Optional[model.Vector] = None,
        end_dir: Optional[model.Vector] = None,
        middle_pos: Optional[model.IPositionable] = None,
        autosplit: Literal[True] = True
    ) -> Optional[List[objects.Segment]]:
        ...

    def _create_segment(self,
        start_node: model.IPositionable, end_node: model.IPositionable,
        type_: Any, *,
        start_dir: Optional[model.Vector] = None,
        end_dir: Optional[model.Vector] = None,
        middle_pos: Optional[model.IPositionable] = None,
        autosplit: bool = False
    ):
        start_is_node = isinstance(start_node, objects.Node)
        end_is_node = isinstance(end_node, objects.Node)
        start_node_id = start_node.id_ \
            if isinstance(start_node, objects.Node) else 0
        end_node_id = end_node.id_ \
            if isinstance(end_node, objects.Node) else 0
        kwargs = {
            'start_node_id': start_node_id,
            'end_node_id': end_node_id,
            'start_position': None if not start_is_node else start_node.position,
            'end_position': None if not end_is_node else end_node.position,
            'net_options': model.NetOptions.ensure(type_),
            'start_dir': start_dir,
            'end_dir': end_dir,
            'control_point': None if middle_pos is None else middle_pos.position,
            'auto_split': autosplit
        }

        ret = self._remote_call(
            protocol.REMOTE_METHODS['create_segment'],
            model.CreateSegmentMessage(**kwargs)
        )
        if isinstance(ret, model.BaseMessage):
            return objects.Segment.from_message(ret)
        if isinstance(ret, list):
            return [objects.Segment.from_message(_) for _ in ret]
        return None

    _create_segments = functools.partial(_create_segment, autosplit=True)

    def begin_path(self, start_node: model.IPositionable, options: Any):
        return objects.PathBuilder(start_node, options)

    def create_segment(self,
        start_node: model.IPositionable, end_node: model.IPositionable,
        type_: Any, *,
        control_point: Optional[model.IPositionable] = None,
        start_dir: Optional[model.Vector] = None,
        end_dir: Optional[model.Vector] = None
    ) -> Optional[objects.Segment]:
        if (start_dir is None) ^ (end_dir is None):
            raise ValueError("start_dir and end_dir must be both set or both unset")
        if (control_point is None) == (start_dir is None):
            raise ValueError("control_point and start_dir can't be both present")
        return self._create_segment(
            start_node, end_node, type_,
            middle_pos=control_point, start_dir=start_dir, end_dir=end_dir
        )

    def create_segments(self,
        start_node: model.IPositionable, end_node: model.IPositionable,
        type_: Any, *,
        control_point: Optional[model.IPositionable] = None,
        start_dir: Optional[model.Vector] = None,
        end_dir: Optional[model.Vector] = None
    ) -> Optional[List[objects.Segment]]:
        if (start_dir is None) ^ (end_dir is None):
            raise ValueError("start_dir and end_dir must be both set or both unset")
        if (control_point is None) == (start_dir is None):
            raise ValueError("control_point and start_dir can't be both present")
        return self._create_segment(
            start_node, end_node, type_,
            middle_pos=control_point,
            start_dir=start_dir, end_dir=end_dir, autosplit=True
        )

    def get_net_prefab(self, name: str) -> objects.NetPrefab:
        return objects.NetPrefab.from_message(
            self._get_obj(idString=name, type_='net prefab')
        )

    def is_prefab(self, name: str) -> bool:
        return self._remote_call(
            protocol.REMOTE_METHODS['exists_prefab'],
            name
        )

    def terrain_height(self, pos: model.IPositionable) -> float:
        return self._remote_call(
            protocol.REMOTE_METHODS['get_terrain_height'],
            pos.position
        )

    def surface_level(self, pos: model.IPositionable) -> float:
        return self._remote_call(
            protocol.REMOTE_METHODS['get_water_level'],
            pos.position
        )

    def draw_vector(self,
        vector: model.IPositionable, origin: model.IPositionable,
        color: str = 'red', length: float = 20, size: float = 0.1
    ):
        return objects.RenderableObjectHandle(id=self._remote_call(
                protocol.REMOTE_METHODS['render_vector'],
                model.RenderVectorMessage(
                    vector=vector.position,
                    origin=origin.position,
                    color=color,
                    length=length,
                    size=size
                )
            )
        )

    def draw_circle(self,
        position: model.IPositionable, radius: float = 5, color: str = 'red'
    ):
        return objects.RenderableObjectHandle(id=self._remote_call(
                protocol.REMOTE_METHODS['render_circle'],
                model.RenderCircleMessage(
                    position=position.position,
                    radius=float(radius),
                    color=color
                )
            )
        )

    def remove_render_object(self, id_: int):
        return self._remote_call(
            protocol.REMOTE_METHODS['remove_render_object'], id_
        )


    def clear(self):
        return self.remove_render_object(0)
